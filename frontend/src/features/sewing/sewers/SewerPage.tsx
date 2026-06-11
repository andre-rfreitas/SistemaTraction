import { useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useSewers, useCreateSewer, useUpdateSewer, useDeleteSewer } from './hooks/useSewers'
import { SewerForm } from './components/SewerForm'
import type { SewerDto } from './types'

export function SewerPage() {
  const { data: sewers = [], isLoading } = useSewers(true)
  const createSewer = useCreateSewer()
  const updateSewer = useUpdateSewer()
  const deleteSewer = useDeleteSewer()

  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<SewerDto | null>(null)

  function handleEdit(sewer: SewerDto) {
    setEditing(sewer)
    setFormOpen(true)
  }

  function handleNew() {
    setEditing(null)
    setFormOpen(true)
  }

  function handleClose() {
    setFormOpen(false)
    setEditing(null)
  }

  const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

  return (
    <div className="space-y-6">
      <PageHeader
        title="Costureiras"
        description="Gerencie as costureiras e seus tipos de produto."
        actions={<Button onClick={handleNew}><Plus className="size-4 mr-1" /> Nova costureira</Button>}
      />

      {isLoading ? (
        <p className="text-muted-foreground text-sm">Carregando...</p>
      ) : sewers.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-8 text-center">
          <p className="text-muted-foreground text-sm">Nenhuma costureira cadastrada.</p>
          <Button variant="outline" className="mt-4" onClick={handleNew}>
            <Plus className="size-4 mr-1" /> Cadastrar costureira
          </Button>
        </div>
      ) : (
        <div className="space-y-3">
          {sewers.map((sewer) => (
            <div key={sewer.id} className="rounded-lg border border-border bg-card p-4 space-y-3">
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="font-semibold text-foreground">{sewer.name}</span>
                  {!sewer.isActive && <Badge variant="neutral">Inativa</Badge>}
                  {sewer.phone && (
                    <span className="text-xs text-muted-foreground">{sewer.phone}</span>
                  )}
                </div>
                <div className="flex gap-1 shrink-0">
                  <Button size="icon" variant="ghost" onClick={() => handleEdit(sewer)}>
                    <Pencil className="size-4" />
                  </Button>
                  <Button
                    size="icon"
                    variant="ghost"
                    className="text-destructive hover:text-destructive"
                    onClick={() => {
                      if (confirm(`Desativar costureira "${sewer.name}"?`))
                        deleteSewer.mutate(sewer.id)
                    }}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              </div>

              {sewer.productTypes.length > 0 ? (
                <div className="border border-border rounded-md overflow-hidden">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="bg-muted/50 text-muted-foreground">
                        <th className="text-left px-3 py-2 font-medium">Produto</th>
                        <th className="text-right px-3 py-2 font-medium">P/M/G/GG</th>
                        <th className="text-right px-3 py-2 font-medium">G1</th>
                      </tr>
                    </thead>
                    <tbody>
                      {sewer.productTypes.map((pt) => (
                        <tr key={pt.id} className="border-t border-border">
                          <td className="px-3 py-2 text-foreground">{pt.name}</td>
                          <td className="px-3 py-2 text-right">R$ {fmt(pt.priceDefault)}</td>
                          <td className="px-3 py-2 text-right">R$ {fmt(pt.priceG1)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="text-xs text-muted-foreground italic">Nenhum tipo de produto cadastrado.</p>
              )}
            </div>
          ))}
        </div>
      )}

      <Dialog open={formOpen} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{editing ? `Editar — ${editing.name}` : 'Nova costureira'}</DialogTitle>
          </DialogHeader>
          <SewerForm
            defaultValues={editing ?? undefined}
            isLoading={createSewer.isPending || updateSewer.isPending}
            onSubmit={(data) => {
              if (editing) {
                updateSewer.mutate(
                  { id: editing.id, input: data },
                  { onSuccess: handleClose }
                )
              } else {
                createSewer.mutate(data, { onSuccess: handleClose })
              }
            }}
            onCancel={handleClose}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
