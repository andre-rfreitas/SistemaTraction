import { useState } from 'react'
import type { FinancialEntryDto } from '../types'
import { formatBRL, formatDate } from '../format'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

interface Props {
  entries: FinancialEntryDto[]
  onReverse: (id: string) => void
  isReversing: boolean
}

export function EntriesTable({ entries, onReverse, isReversing }: Props) {
  const [toReverse, setToReverse] = useState<FinancialEntryDto | null>(null)

  if (entries.length === 0) {
    return (
      <div className="rounded-lg border border-neutral-200 bg-white p-6 text-sm text-neutral-500">
        Nenhum lançamento encontrado para os filtros selecionados.
      </div>
    )
  }

  return (
    <div className="rounded-lg border border-neutral-200 bg-white overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-neutral-50 text-neutral-500">
          <tr className="text-left">
            <th className="px-4 py-2 font-medium">Data</th>
            <th className="px-4 py-2 font-medium">Categoria</th>
            <th className="px-4 py-2 font-medium">Descrição</th>
            <th className="px-4 py-2 font-medium text-right">Valor</th>
            <th className="px-4 py-2 font-medium text-right">Ação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-neutral-100">
          {entries.map((e) => (
            <tr key={e.id} className={e.isReversal ? 'bg-neutral-50/60' : ''}>
              <td className="px-4 py-2 text-neutral-600 whitespace-nowrap">{formatDate(e.entryDate)}</td>
              <td className="px-4 py-2">
                <Badge variant={e.type === 'Income' ? 'default' : 'secondary'}>{e.category}</Badge>
              </td>
              <td className="px-4 py-2 text-neutral-700">
                {e.description}
                {e.isReversal && (
                  <span className="ml-2 text-xs text-neutral-400">(estorno)</span>
                )}
                {e.isReversed && !e.isReversal && (
                  <span className="ml-2 text-xs text-red-400">(estornado)</span>
                )}
              </td>
              <td
                className={`px-4 py-2 text-right font-medium whitespace-nowrap ${
                  e.amount < 0
                    ? 'text-neutral-400'
                    : e.type === 'Income'
                      ? 'text-green-700'
                      : 'text-neutral-900'
                }`}
              >
                {e.type === 'Income' ? '+' : '−'}
                {formatBRL(Math.abs(e.amount))}
              </td>
              <td className="px-4 py-2 text-right">
                {!e.isReversal && !e.isReversed && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-600 hover:text-red-700"
                    onClick={() => setToReverse(e)}
                  >
                    Estornar
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <Dialog open={!!toReverse} onOpenChange={(v) => { if (!v) setToReverse(null) }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Estornar lançamento</DialogTitle>
          </DialogHeader>
          <div className="text-sm text-neutral-600 space-y-2">
            <p>
              Lançamentos financeiros não podem ser excluídos. O estorno cria um novo
              lançamento de valor oposto, mantendo a rastreabilidade.
            </p>
            {toReverse && (
              <div className="rounded-md bg-neutral-50 border border-neutral-200 p-3">
                <p className="font-medium text-neutral-900">{toReverse.description}</p>
                <p className="text-neutral-500">
                  {toReverse.category} — {formatBRL(toReverse.amount)}
                </p>
              </div>
            )}
          </div>
          <div className="flex justify-end gap-2 mt-2">
            <Button variant="outline" onClick={() => setToReverse(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              disabled={isReversing}
              onClick={() => {
                if (toReverse) {
                  onReverse(toReverse.id)
                  setToReverse(null)
                }
              }}
            >
              {isReversing ? 'Estornando...' : 'Confirmar estorno'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
