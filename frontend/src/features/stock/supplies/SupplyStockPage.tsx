import { useState } from 'react'
import { SupplyStockList } from './components/SupplyStockList'
import { SupplyMovementForm } from './components/SupplyMovementForm'
import { SupplyMovementHistory } from './components/SupplyMovementHistory'
import { useRegisterSupplyMovement } from './hooks/useRegisterSupplyMovement'
import { useCreateFinancialEntry } from '@/features/financial/hooks/useCreateFinancialEntry'
import type { SupplyStockItemDto, RegisterSupplyMovementResult } from './types'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

type DialogStep = 'movement' | 'financial-confirm'

export function SupplyStockPage() {
  const [selected, setSelected] = useState<SupplyStockItemDto | null>(null)
  const [step, setStep] = useState<DialogStep>('movement')
  const [movementResult, setMovementResult] = useState<RegisterSupplyMovementResult | null>(null)
  const [financialAmount, setFinancialAmount] = useState('')
  const [financialDesc, setFinancialDesc] = useState('')

  const register = useRegisterSupplyMovement()
  const createEntry = useCreateFinancialEntry()

  function handleClose() {
    setSelected(null)
    setStep('movement')
    setMovementResult(null)
    setFinancialAmount('')
    setFinancialDesc('')
  }

  function handleMovementSubmit(data: { type: 'Entrada' | 'Saida' | 'Ajuste'; quantity: number; reason?: string | null }) {
    if (!selected) return
    register.mutate(
      { supplyStockItemId: selected.id, ...data },
      {
        onSuccess: (result) => {
          if (result.requiresFinancialConfirmation) {
            setMovementResult(result)
            setFinancialDesc(result.suggestedDescription ?? '')
            setStep('financial-confirm')
          } else {
            handleClose()
          }
        },
      }
    )
  }

  function handleFinancialConfirm() {
    if (!selected || !financialAmount) return
    createEntry.mutate(
      {
        type: 'Expense',
        category: selected.name,
        amount: parseFloat(financialAmount),
        description: financialDesc,
      },
      { onSuccess: handleClose }
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Estoque de Embalagens"
        description="Posição atual de insumos por tipo. Registre entradas, saídas e ajustes."
      />

      <SupplyStockList onSelect={(item) => { setSelected(item); setStep('movement') }} />

      <Dialog open={!!selected} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{selected?.name}</DialogTitle>
          </DialogHeader>

          {step === 'movement' && selected && (
            <div className="space-y-5">
              <div>
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                  Registrar movimento
                </p>
                <SupplyMovementForm
                  isLoading={register.isPending}
                  onSubmit={handleMovementSubmit}
                />
                {register.isError && (
                  <p className="text-sm text-danger mt-2">
                    {(register.error as Error).message}
                  </p>
                )}
              </div>

              <div>
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                  Últimas movimentações
                </p>
                <SupplyMovementHistory supplyStockItemId={selected.id} />
              </div>
            </div>
          )}

          {step === 'financial-confirm' && movementResult && (
            <div className="space-y-4">
              <div className="rounded-md bg-success/10 border border-success/20 p-3 text-sm text-success">
                ✓ Entrada registrada com sucesso!
              </div>
              <p className="text-sm text-foreground font-medium">
                Deseja registrar a despesa no financeiro?
              </p>
              <div className="space-y-3">
                <div className="space-y-1">
                  <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Valor (R$)</label>
                  <Input
                    type="number"
                    step="0.01"
                    min="0.01"
                    value={financialAmount}
                    onChange={(e) => setFinancialAmount(e.target.value)}
                    placeholder="0,00"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Descrição</label>
                  <Input
                    value={financialDesc}
                    onChange={(e) => setFinancialDesc(e.target.value)}
                  />
                </div>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" onClick={handleClose} className="flex-1">
                  Não registrar
                </Button>
                <Button
                  onClick={handleFinancialConfirm}
                  disabled={!financialAmount || createEntry.isPending}
                  className="flex-1"
                >
                  {createEntry.isPending ? 'Registrando...' : 'Registrar despesa'}
                </Button>
              </div>
              {createEntry.isError && (
                <p className="text-xs text-danger">Erro ao registrar despesa financeira.</p>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
