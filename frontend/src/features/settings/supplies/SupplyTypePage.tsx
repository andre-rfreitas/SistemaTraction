import { useState } from 'react'
import { SupplyTypeList } from './components/SupplyTypeList'
import { SupplyTypeForm } from './components/SupplyTypeForm'
import { useCreateSupplyType } from './hooks/useCreateSupplyType'
import { useUpdateSupplyType } from './hooks/useUpdateSupplyType'
import type { SupplyTypeDto } from './types'
import { Button } from '@/components/ui/button'
import { PageHeader } from '@/components/ui/page-header'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

export function SupplyTypePage() {
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<SupplyTypeDto | null>(null)
  const create = useCreateSupplyType()
  const update = useUpdateSupplyType()

  function handleEdit(item: SupplyTypeDto) {
    setEditing(item)
    setOpen(true)
  }

  function handleClose() {
    setOpen(false)
    setEditing(null)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tipos de Insumo"
        description="Gerencie os tipos de embalagens, etiquetas e materiais de envio."
        actions={
          <Button onClick={() => { setEditing(null); setOpen(true) }}>
            + Novo tipo
          </Button>
        }
      />

      <SupplyTypeList onEdit={handleEdit} />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar tipo' : 'Novo tipo de insumo'}</DialogTitle>
          </DialogHeader>
          <SupplyTypeForm
            defaultValues={editing ?? undefined}
            isLoading={create.isPending || update.isPending}
            onSubmit={(data) => {
              if (editing) {
                update.mutate({ id: editing.id, data }, { onSuccess: handleClose })
              } else {
                create.mutate(data, { onSuccess: handleClose })
              }
            }}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
