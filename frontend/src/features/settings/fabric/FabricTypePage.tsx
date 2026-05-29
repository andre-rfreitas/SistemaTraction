import { useState } from 'react'
import { FabricTypeList } from './components/FabricTypeList'
import { FabricTypeForm } from './components/FabricTypeForm'
import { useCreateFabricType } from './hooks/useCreateFabricType'
import { useUpdateFabricType } from './hooks/useUpdateFabricType'
import type { FabricTypeDto } from './types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

export function FabricTypePage() {
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<FabricTypeDto | null>(null)
  const createType = useCreateFabricType()
  const updateType = useUpdateFabricType()

  function handleEdit(type: FabricTypeDto) {
    setEditing(type)
    setOpen(true)
  }

  function handleClose() {
    setOpen(false)
    setEditing(null)
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-xl font-bold text-neutral-900">Tipos de Tecido</h2>
          <p className="text-sm text-neutral-500">
            Gerencie tipos, variações e cores dos tecidos usados na produção.
          </p>
        </div>
        <Button onClick={() => { setEditing(null); setOpen(true) }}>+ Novo tipo</Button>
      </div>

      <FabricTypeList onEdit={handleEdit} />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? 'Editar tipo de tecido' : 'Novo tipo de tecido'}
            </DialogTitle>
          </DialogHeader>
          <FabricTypeForm
            defaultValues={editing ?? undefined}
            isLoading={createType.isPending || updateType.isPending}
            onSubmit={(data) => {
              if (editing) {
                updateType.mutate(
                  { id: editing.id, data },
                  { onSuccess: handleClose }
                )
              } else {
                createType.mutate(data, { onSuccess: handleClose })
              }
            }}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
