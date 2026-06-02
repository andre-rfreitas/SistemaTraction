import { useState } from 'react'
import { Inbox } from 'lucide-react'
import type { FinancialEntryDto } from '../types'
import { formatBRL, formatDate } from '../format'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell,
} from '@/components/ui/table'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
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
      <EmptyState
        icon={Inbox}
        title="Nenhum lançamento"
        description="Nenhum lançamento encontrado para os filtros selecionados."
      />
    )
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Data</TableHead>
            <TableHead>Categoria</TableHead>
            <TableHead>Descrição</TableHead>
            <TableHead className="text-right">Valor</TableHead>
            <TableHead className="text-right">Ação</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {entries.map((e) => (
            <TableRow key={e.id} className={e.isReversal ? 'bg-muted/40' : undefined}>
              <TableCell className="whitespace-nowrap text-muted-foreground">
                {formatDate(e.entryDate)}
              </TableCell>
              <TableCell>
                <Badge variant={e.type === 'Income' ? 'success' : 'neutral'}>{e.category}</Badge>
              </TableCell>
              <TableCell className="text-foreground">
                {e.description}
                {e.isReversal && <span className="ml-2 text-xs text-muted-foreground">(estorno)</span>}
                {e.isReversed && !e.isReversal && (
                  <span className="ml-2 text-xs text-danger">(estornado)</span>
                )}
              </TableCell>
              <TableCell
                className={`whitespace-nowrap text-right font-medium tabular-nums ${
                  e.amount < 0
                    ? 'text-muted-foreground'
                    : e.type === 'Income'
                      ? 'text-success'
                      : 'text-foreground'
                }`}
              >
                {e.type === 'Income' ? '+' : '−'}
                {formatBRL(Math.abs(e.amount))}
              </TableCell>
              <TableCell className="text-right">
                {!e.isReversal && !e.isReversed && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive/80"
                    onClick={() => setToReverse(e)}
                  >
                    Estornar
                  </Button>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={toReverse !== null} onOpenChange={(o) => !o && setToReverse(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Estornar lançamento</DialogTitle>
          </DialogHeader>
          <div className="text-sm text-muted-foreground space-y-2">
            <p>
              Lançamentos financeiros não podem ser excluídos. O estorno cria um novo
              lançamento de valor oposto, mantendo a rastreabilidade.
            </p>
            {toReverse && (
              <div className="rounded-md bg-muted border p-3">
                <p className="font-medium text-foreground">{toReverse.description}</p>
                <p className="text-muted-foreground">
                  {toReverse.category} — {formatBRL(toReverse.amount)}
                </p>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setToReverse(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              isLoading={isReversing}
              onClick={() => {
                if (toReverse) {
                  onReverse(toReverse.id)
                  setToReverse(null)
                }
              }}
            >
              Confirmar estorno
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}
