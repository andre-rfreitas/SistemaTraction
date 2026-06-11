import { useState } from 'react'
import { FabricRollList } from './components/FabricRollList'
import { FabricRollForm } from './components/FabricRollForm'
import { useRegisterFabricRoll } from './hooks/useRegisterFabricRoll'
import { useUpdateFabricRoll } from './hooks/useUpdateFabricRoll'
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
  const register = useRegisterFabricRoll()
  const update = useUpdateFabricRoll()

  function handleClose() {
    setOpen(false)
    setEditingRoll(null)
    setSummary(null)
  }

  function handleEdit(roll: FabricRollDto) {
    setEditingRoll(roll)
    setOpen(true)
  }

  const isEditing = editingRoll !== null

  return (
    <div className="space-y-6">
      <PageHeader
        title="Chegada de Bobina"
        description="Registre a entrada de bobinas de tecido e acompanhe o histórico."
        actions={<Button onClick={() => setOpen(true)}>+ Nova bobina</Button>}
      />

      <FabricRollList onEdit={handleEdit} />

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
    </div>
  )
}
