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

function ShopifyBadge({ sourceName }: { sourceName: string }) {
  const label = sourceName.includes('tiktok') ? 'Shopify — TikTok' : 'Shopify — Web'
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-[#96BF48]/10 px-1.5 py-0.5 text-[10px] font-medium text-[#5a8c0e]">
      <svg viewBox="0 0 24 24" className="h-2.5 w-2.5 shrink-0" fill="currentColor">
        <path d="M15.337 3.054c-.07 0-.14.003-.213.008a2.944 2.944 0 00-.588.1 2.29 2.29 0 00-.823.415c-.508-.102-1.128.159-1.581.989l-.04.075-.044.07C11.638 5.1 11.3 5.9 11.3 7c0 .028 0 .057.002.086l4.784 1.479c.048-1.871-.46-4.068-.749-5.511zM5.596 22.5l13.808-2.5-4.696-16.517-9.112-1.039L5.596 22.5z" />
      </svg>
      {label}
    </span>
  )
}

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
                <div className="flex flex-col gap-1">
                  <Badge variant={e.type === 'Income' ? 'success' : 'neutral'}>{e.category}</Badge>
                  {e.referenceType === 'ShopifyOrder' && (
                    <ShopifyBadge sourceName={e.category} />
                  )}
                </div>
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
                {!e.isReversal && !e.isReversed && e.referenceType !== 'ShopifyOrder' && (
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
            {toReverse?.referenceType === 'CuttingDelivery' && (
              <p className="text-amber-600 font-medium">
                Este estorno também cancelará o pedido de corte vinculado e reverterá as bobinas de tecido para disponível.
              </p>
            )}
            {toReverse?.referenceType === 'SewingDelivery' && (
              <p className="text-amber-600 font-medium">
                Este estorno também cancelará o pedido de corte, reverterá o estoque de camisetas e estornará todos os lançamentos vinculados (Costura e Defeitos).
              </p>
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
