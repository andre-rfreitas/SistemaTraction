import { useState } from 'react'
import { FabricRollList } from './components/FabricRollList'
import { FabricRollForm } from './components/FabricRollForm'
import { useRegisterFabricRoll } from './hooks/useRegisterFabricRoll'
import type { RegisterFabricRollResult } from './types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

export function FabricRollPage() {
  const [open, setOpen] = useState(false)
  const [summary, setSummary] = useState<RegisterFabricRollResult | null>(null)
  const register = useRegisterFabricRoll()

  function handleClose() {
    setOpen(false)
    setSummary(null)
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-xl font-bold text-neutral-900">Chegada de Bobina</h2>
          <p className="text-sm text-neutral-500">
            Registre a entrada de bobinas de tecido e acompanhe o histórico.
          </p>
        </div>
        <Button onClick={() => setOpen(true)}>+ Nova bobina</Button>
      </div>

      <FabricRollList />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Registrar chegada de bobina</DialogTitle>
          </DialogHeader>

          {summary ? (
            <div className="space-y-4">
              <div className="rounded-md bg-green-50 border border-green-200 p-4 text-sm space-y-2">
                <p className="font-semibold text-green-800">Bobina registrada com sucesso!</p>
                <div className="flex justify-between text-green-700">
                  <span>Lançamento financeiro (Tecido):</span>
                  <span className="font-medium">
                    R$ {summary.financialEntryAmount.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </span>
                </div>
                <p className="text-xs text-green-600">
                  Calculado com base no preço/kg cadastrado no tipo de tecido (R${' '}
                  {summary.fabricTypePricePerKg.toFixed(4)}/kg), não no preço real pago.
                </p>
              </div>
              <Button onClick={handleClose} className="w-full">
                Fechar
              </Button>
            </div>
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
