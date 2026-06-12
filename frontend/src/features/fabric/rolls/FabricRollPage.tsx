import { useState } from 'react'
import { FabricRollList } from './components/FabricRollList'
import { FabricRollForm } from './components/FabricRollForm'
import { useRegisterFabricRoll } from './hooks/useRegisterFabricRoll'
import { useUpdateFabricRoll } from './hooks/useUpdateFabricRoll'
import { useDeleteFabricRoll } from './hooks/useDeleteFabricRoll'
import type { FabricRollDto, RegisterFabricRollResult } from './types'
import { Button } from '@/components/ui/button'
import { PageHeader } from '@/components/ui/page-header'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

export function FabricRollPage() {
  const [open, setOpen] = useState(false)
  const [editingRoll, setEditingRoll] = useState<FabricRollDto | null>(null)
  const [summary, setSummary] = useState<RegisterFabricRollResult | null>(null)
  const [deletingRoll, setDeletingRoll] = useState<FabricRollDto | null>(null)

  const register = useRegisterFabricRoll()
  const update = useUpdateFabricRoll()
  const deleteMutation = useDeleteFabricRoll()

  function handleClose() {
    setOpen(false)
    setEditingRoll(null)
    setSummary(null)
  }

  function handleEdit(roll: FabricRollDto) {
    setEditingRoll(roll)
    setOpen(true)
  }

  function handleDeleteConfirm() {
    if (!deletingRoll) return
    deleteMutation.mutate(deletingRoll.id, {
      onSuccess: () => setDeletingRoll(null),
    })
  }

  const isEditing = editingRoll !== null

  return (
    <div className="space-y-6">
      <PageHeader
        title="Chegada de Bobina"
        description="Registre a entrada de bobinas de tecido e acompanhe o histórico."
        actions={<Button onClick={() => setOpen(true)}>+ Nova bobina</Button>}
      />

      <FabricRollList onEdit={handleEdit} onDelete={(r) => setDeletingRoll(r)} />

      {/* Dialog: registrar / editar */}
      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {isEditing ? 'Editar bobina' : 'Registrar chegada de bobina'}
            </DialogTitle>
          </DialogHeader>

          {summary ? (
            <div className="space-y-4">
              <div className="rounded-md bg-success/10 border border-success/20 p-4 text-sm space-y-2">
                <p className="font-semibold text-success">Bobina registrada com sucesso!</p>
                <div className="flex justify-between text-success">
                  <span>Lançamento financeiro (Tecido):</span>
                  <span className="font-medium">
                    R$ {summary.financialEntryAmount.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </span>
                </div>
                <p className="text-xs text-success/80">
                  Calculado com base no preço/kg cadastrado no tipo de tecido (R${' '}
                  {summary.fabricTypePricePerKg.toFixed(4)}/kg), não no preço real pago.
                </p>
              </div>
              <Button onClick={handleClose} className="w-full">
                Fechar
              </Button>
            </div>
          ) : isEditing ? (
            <FabricRollForm
              key={editingRoll.id}
              isLoading={update.isPending}
              submitLabel="Salvar alterações"
              defaultValues={{
                fabricTypeId: editingRoll.fabricTypeId,
                fabricColorId: editingRoll.fabricColorId,
                weightKg: editingRoll.weightKg,
                priceTotal: editingRoll.priceTotal,
              }}
              onSubmit={(data) => {
                update.mutate(
                  { id: editingRoll.id, data },
                  { onSuccess: handleClose },
                )
              }}
            />
          ) : (
            <FabricRollForm
              isLoading={register.isPending}
              onSubmit={(data) => {
                register.mutate(data, {
                  onSuccess: (result) => setSummary(result),
                })
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Dialog: confirmar exclusão */}
      <Dialog open={!!deletingRoll} onOpenChange={(v) => { if (!v) setDeletingRoll(null) }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Excluir bobina?</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            {deletingRoll && (
              <p className="text-sm text-muted-foreground">
                Excluir{' '}
                <span className="font-medium text-foreground">
                  {deletingRoll.fabricColorName} — {deletingRoll.fabricTypeName} {deletingRoll.fabricTypeVariation}
                </span>
                {' '}({deletingRoll.weightKg.toFixed(3)} kg)?
                {deletingRoll.status === 'Consumed' && (
                  <span className="block mt-1 text-xs text-warning">
                    Esta bobina já foi consumida. O histórico financeiro será mantido.
                  </span>
                )}
              </p>
            )}
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => setDeletingRoll(null)}
                disabled={deleteMutation.isPending}
                className="flex-1"
              >
                Cancelar
              </Button>
              <Button
                variant="destructive"
                onClick={handleDeleteConfirm}
                disabled={deleteMutation.isPending}
                className="flex-1"
              >
                {deleteMutation.isPending ? 'Excluindo...' : 'Excluir'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
