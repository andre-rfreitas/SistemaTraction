import { useState } from 'react'
import { SupplyStockList } from './components/SupplyStockList'
import { SupplyMovementForm } from './components/SupplyMovementForm'
import { SupplyMovementHistory } from './components/SupplyMovementHistory'
import { SupplyEntryForm, type EntryFormData } from './components/SupplyEntryForm'
import { useRegisterSupplyMovement } from './hooks/useRegisterSupplyMovement'
import { useUpdateSupplyMovement } from './hooks/useUpdateSupplyMovement'
import { useCreateFinancialEntry } from '@/features/financial/hooks/useCreateFinancialEntry'
import type { SupplyStockItemDto, SupplyStockMovementDto, RegisterSupplyMovementResult } from './types'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

type DialogMode =
  | { kind: 'entry'; item: SupplyStockItemDto }
  | { kind: 'edit-entry'; item: SupplyStockItemDto; movement: SupplyStockMovementDto }
  | { kind: 'movement'; item: SupplyStockItemDto }
  | { kind: 'financial'; item: SupplyStockItemDto; result: RegisterSupplyMovementResult; suggestedAmount?: number }
  | null

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

export function SupplyStockPage() {
  const [dialog, setDialog] = useState<DialogMode>(null)
  const [financialAmount, setFinancialAmount] = useState('')
  const [financialDesc, setFinancialDesc] = useState('')

  const register = useRegisterSupplyMovement()
  const updateMovement = useUpdateSupplyMovement(
    dialog?.kind === 'edit-entry' ? dialog.item.id : ''
  )
  const createEntry = useCreateFinancialEntry()

  function closeDialog() {
    setDialog(null)
    setFinancialAmount('')
    setFinancialDesc('')
  }

  function handleEntry(item: SupplyStockItemDto, data: EntryFormData) {
    register.mutate(
      {
        supplyStockItemId: item.id,
        type: 'Entrada',
        quantity: data.quantity,
        reason: null,
        supplierName: data.supplierName || null,
        supplierPhone: data.supplierPhone || null,
        occurredAt: data.occurredAt ? new Date(data.occurredAt).toISOString() : undefined,
        unitPrice: data.unitPrice ?? undefined,
        totalCost: data.totalCost ?? undefined,
      },
      {
        onSuccess: (result) => {
          if (result.requiresFinancialConfirmation) {
            setFinancialDesc(result.suggestedDescription ?? `Compra: ${item.name}`)
            setFinancialAmount(data.totalCost != null ? String(data.totalCost.toFixed(2)) : '')
            setDialog({ kind: 'financial', item, result, suggestedAmount: data.totalCost ?? undefined })
          } else {
            closeDialog()
          }
        },
      }
    )
  }

  function handleEditSubmit(data: EntryFormData) {
    if (dialog?.kind !== 'edit-entry') return
    updateMovement.mutate(
      {
        movementId: dialog.movement.id,
        data: {
          supplierName: data.supplierName || null,
          supplierPhone: data.supplierPhone || null,
          occurredAt: data.occurredAt,
          unitPrice: data.unitPrice,
          totalCost: data.totalCost,
        },
      },
      { onSuccess: closeDialog }
    )
  }

  function handleMovementSubmit(data: { type: 'Entrada' | 'Saida' | 'Ajuste'; quantity: number; reason?: string | null }) {
    if (dialog?.kind !== 'movement') return
    const item = dialog.item
    register.mutate(
      { supplyStockItemId: item.id, ...data },
      {
        onSuccess: (result) => {
          if (result.requiresFinancialConfirmation) {
            setFinancialDesc(result.suggestedDescription ?? '')
            setFinancialAmount(result.suggestedAmount != null ? String(result.suggestedAmount.toFixed(2)) : '')
            setDialog({ kind: 'financial', item, result, suggestedAmount: result.suggestedAmount ?? undefined })
          } else {
            closeDialog()
          }
        },
      }
    )
  }

  function handleFinancialConfirm() {
    if (dialog?.kind !== 'financial') return
    const item = dialog.item
    createEntry.mutate(
      {
        type: 'Expense',
        category: item.name,
        amount: parseFloat(financialAmount),
        description: financialDesc,
      },
      { onSuccess: closeDialog }
    )
  }

  const dialogOpen = dialog !== null

  function dialogTitle() {
    if (!dialog) return ''
    if (dialog.kind === 'entry') return `Registrar entrada — ${dialog.item.name}`
    if (dialog.kind === 'edit-entry') return `Editar entrada — ${dialog.item.name}`
    if (dialog.kind === 'movement') return dialog.item.name
    if (dialog.kind === 'financial') return 'Registrar despesa?'
    return ''
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Insumos e Embalagens"
        description="Posição atual de insumos por tipo. Registre entradas, saídas e ajustes."
      />

      <SupplyStockList
        onEntry={(item) => setDialog({ kind: 'entry', item })}
        onMovement={(item) => setDialog({ kind: 'movement', item })}
      />

      <Dialog open={dialogOpen} onOpenChange={(v) => { if (!v) closeDialog() }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{dialogTitle()}</DialogTitle>
          </DialogHeader>

          {dialog?.kind === 'entry' && (
            <SupplyEntryForm
              item={dialog.item}
              isLoading={register.isPending}
              onSubmit={(data) => handleEntry(dialog.item, data)}
              onCancel={closeDialog}
            />
          )}

          {dialog?.kind === 'edit-entry' && (
            <SupplyEntryForm
              item={dialog.item}
              editing={dialog.movement}
              isLoading={updateMovement.isPending}
              onSubmit={handleEditSubmit}
              onCancel={closeDialog}
            />
          )}

          {dialog?.kind === 'movement' && (
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
                <SupplyMovementHistory
                  supplyStockItemId={dialog.item.id}
                  onEdit={(movement) =>
                    setDialog({ kind: 'edit-entry', item: dialog.item, movement })
                  }
                />
              </div>
            </div>
          )}

          {dialog?.kind === 'financial' && (
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
                  {dialog.suggestedAmount != null && !financialAmount && (
                    <p className="text-xs text-muted-foreground">
                      Sugerido: R$ {fmt(dialog.suggestedAmount)}
                      {' '}
                      <button
                        type="button"
                        className="text-primary underline"
                        onClick={() => setFinancialAmount(dialog.suggestedAmount!.toFixed(2))}
                      >
                        Usar
                      </button>
                    </p>
                  )}
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
                <Button variant="outline" onClick={closeDialog} className="flex-1">
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
