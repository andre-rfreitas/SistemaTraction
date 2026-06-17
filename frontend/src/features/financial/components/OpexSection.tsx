import { useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { useOperationalExpenses, useCreateOpex, useUpdateOpex, useDeleteOpex } from '../hooks/useOperationalExpenses'
import { OpexForm } from './OpexForm'
import type { OperationalExpenseDto, OpexPeriodItemDto } from '../types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

interface Props {
  opexItems: OpexPeriodItemDto[]
  totalOpex: number
}

export function OpexSection({ opexItems, totalOpex }: Props) {
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<OperationalExpenseDto | null>(null)
  const [confirmDelete, setConfirmDelete] = useState<OperationalExpenseDto | null>(null)

  const { data: expenses = [] } = useOperationalExpenses()
  const create = useCreateOpex()
  const update = useUpdateOpex()
  const del = useDeleteOpex()

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-semibold text-foreground uppercase tracking-wide">Despesas de Operação (OPEX)</h3>
          {totalOpex > 0 && (
            <p className="text-xs text-muted-foreground mt-0.5">
              Total no período: <span className="font-semibold text-foreground">R$ {fmt(totalOpex)}</span>
            </p>
          )}
        </div>
        <Button size="sm" variant="outline" onClick={() => setShowCreate(true)} className="gap-1.5 text-xs h-8">
          <Plus className="h-3.5 w-3.5" />
          Cadastrar despesa
        </Button>
      </div>

      {expenses.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted-foreground">
          Nenhuma despesa operacional cadastrada.<br />
          <span className="text-xs">Ex: Energia, Folha de pagamento, Taxas de plataformas</span>
        </div>
      ) : (
        <div className="rounded-lg border border-border overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/30">
                <th className="text-left px-3 py-2 text-xs font-medium text-muted-foreground">Despesa</th>
                <th className="text-right px-3 py-2 text-xs font-medium text-muted-foreground">Fixo mensal</th>
                <th className="text-right px-3 py-2 text-xs font-medium text-muted-foreground">Taxa</th>
                <th className="text-right px-3 py-2 text-xs font-medium text-muted-foreground">No período</th>
                <th className="px-3 py-2 w-16" />
              </tr>
            </thead>
            <tbody>
              {expenses.map((expense) => {
                const periodItem = opexItems.find((o) => o.id === expense.id)
                return (
                  <tr key={expense.id} className={`border-b border-border last:border-0 ${!expense.isActive ? 'opacity-40' : ''}`}>
                    <td className="px-3 py-2.5 font-medium text-foreground">
                      {expense.name}
                      {!expense.isActive && <span className="ml-1.5 text-xs text-muted-foreground">(inativo)</span>}
                    </td>
                    <td className="px-3 py-2.5 text-right text-muted-foreground">
                      {expense.fixedMonthlyValue > 0 ? `R$ ${fmt(expense.fixedMonthlyValue)}` : '—'}
                    </td>
                    <td className="px-3 py-2.5 text-right text-muted-foreground">
                      {expense.ratePercent > 0 ? `${expense.ratePercent}%` : '—'}
                    </td>
                    <td className="px-3 py-2.5 text-right font-semibold text-foreground">
                      {periodItem ? `R$ ${fmt(periodItem.periodTotal)}` : '—'}
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex items-center gap-1 justify-end">
                        <button
                          onClick={() => setEditing(expense)}
                          className="p-1 rounded hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                          title="Editar"
                        >
                          <Pencil className="h-3.5 w-3.5" />
                        </button>
                        <button
                          onClick={() => setConfirmDelete(expense)}
                          className="p-1 rounded hover:bg-destructive/10 text-muted-foreground hover:text-destructive transition-colors"
                          title="Excluir"
                        >
                          <Trash2 className="h-3.5 w-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
            {totalOpex > 0 && (
              <tfoot>
                <tr className="border-t border-border bg-muted/20">
                  <td colSpan={3} className="px-3 py-2 text-xs font-medium text-muted-foreground">Total OPEX no período</td>
                  <td className="px-3 py-2 text-right text-sm font-bold text-foreground">R$ {fmt(totalOpex)}</td>
                  <td />
                </tr>
              </tfoot>
            )}
          </table>
        </div>
      )}

      {/* Dialog: criar */}
      <Dialog open={showCreate} onOpenChange={(v) => { if (!v) setShowCreate(false) }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Cadastrar despesa operacional</DialogTitle>
          </DialogHeader>
          <OpexForm
            isLoading={create.isPending}
            onCancel={() => setShowCreate(false)}
            onSubmit={(data) =>
              create.mutate(
                { name: data.name, fixedMonthlyValue: data.fixedMonthlyValue, ratePercent: data.ratePercent },
                { onSuccess: () => setShowCreate(false) }
              )
            }
          />
          {create.isError && <p className="text-xs text-destructive">{(create.error as Error).message}</p>}
        </DialogContent>
      </Dialog>

      {/* Dialog: editar */}
      <Dialog open={!!editing} onOpenChange={(v) => { if (!v) setEditing(null) }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Editar despesa operacional</DialogTitle>
          </DialogHeader>
          {editing && (
            <OpexForm
              editing={editing}
              isLoading={update.isPending}
              onCancel={() => setEditing(null)}
              onSubmit={(data) =>
                update.mutate(
                  { id: editing.id, name: data.name, fixedMonthlyValue: data.fixedMonthlyValue, ratePercent: data.ratePercent, isActive: data.isActive },
                  { onSuccess: () => setEditing(null) }
                )
              }
            />
          )}
          {update.isError && <p className="text-xs text-destructive">{(update.error as Error).message}</p>}
        </DialogContent>
      </Dialog>

      {/* Dialog: confirmar exclusão */}
      <Dialog open={!!confirmDelete} onOpenChange={(v) => { if (!v) setConfirmDelete(null) }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Excluir "{confirmDelete?.name}"?</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">Esta despesa será removida e não afetará mais os cálculos do financeiro.</p>
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setConfirmDelete(null)} className="flex-1" disabled={del.isPending}>
                Cancelar
              </Button>
              <Button
                variant="destructive"
                className="flex-1"
                disabled={del.isPending}
                onClick={() =>
                  del.mutate(confirmDelete!.id, { onSuccess: () => setConfirmDelete(null) })
                }
              >
                {del.isPending ? 'Excluindo...' : 'Excluir'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
