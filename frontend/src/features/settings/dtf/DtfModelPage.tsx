import { useState } from 'react'
import { DtfModelList } from './components/DtfModelList'
import { DtfModelForm } from './components/DtfModelForm'
import { useCreateDtfModel } from './hooks/useCreateDtfModel'
import { useUpdateDtfModel } from './hooks/useUpdateDtfModel'
import type { DtfModelDto } from './types'
import { Button } from '@/components/ui/button'
import { PageHeader } from '@/components/ui/page-header'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

export function DtfModelPage() {
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<DtfModelDto | null>(null)
  const createModel = useCreateDtfModel()
  const updateModel = useUpdateDtfModel()

  function handleEdit(model: DtfModelDto) {
    setEditing(model)
    setOpen(true)
  }

  function handleClose() {
    setOpen(false)
    setEditing(null)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Modelos de DTF"
        description="Gerencie os modelos de estampa DTF, folhas e custo por folha."
        actions={
          <Button
            onClick={() => {
              setEditing(null)
              setOpen(true)
            }}
          >
            + Novo modelo
          </Button>
        }
      />

      <DtfModelList onEdit={handleEdit} />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? 'Editar modelo DTF' : 'Novo modelo DTF'}
            </DialogTitle>
          </DialogHeader>
          <DtfModelForm
            defaultValues={editing ?? undefined}
            isLoading={createModel.isPending || updateModel.isPending}
            onSubmit={(data) => {
              if (editing) {
                updateModel.mutate(
                  { id: editing.id, data },
                  { onSuccess: handleClose }
                )
              } else {
                createModel.mutate(data, { onSuccess: handleClose })
              }
            }}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
