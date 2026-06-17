import { useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useGenericProductMovements } from '../hooks/useGenericProductsApi'
import { cn } from '@/lib/utils'

export function GenericProductMovementsTable({ productId }: { productId: string }) {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useGenericProductMovements(productId, page)

  if (isLoading) {
    return <Skeleton className="h-32 w-full rounded-lg" />
  }

  if (!data || data.items.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-card p-6 text-center text-sm text-muted-foreground">
        Nenhuma movimentação registrada.
      </div>
    )
  }

  const totalPages = Math.ceil(data.totalCount / data.pageSize)

  return (
    <div className="space-y-3">
      <div className="rounded-lg border border-border bg-card overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50">
              <TableHead className="w-[160px]">Data</TableHead>
              <TableHead className="w-[100px] text-right">Qtd.</TableHead>
              <TableHead className="w-[120px]">Origem</TableHead>
              <TableHead>Motivo</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((m) => {
              const isPositive = m.delta > 0
              return (
                <TableRow key={m.id} className="text-sm">
                  <TableCell className="text-muted-foreground">
                    {new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(m.date))}
                  </TableCell>
                  <TableCell className="text-right font-medium">
                    <span className={cn(
                      'inline-flex items-center rounded-md px-2 py-0.5 text-xs font-semibold',
                      isPositive ? 'bg-success/10 text-success' : 'bg-destructive/10 text-destructive'
                    )}>
                      {isPositive ? '+' : ''}{m.delta}
                    </span>
                  </TableCell>
                  <TableCell>
                    <span className="inline-flex rounded bg-muted px-2 py-1 text-xs font-medium text-muted-foreground">
                      {m.origin}
                    </span>
                  </TableCell>
                  <TableCell>{m.reason}</TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between px-2">
          <p className="text-xs text-muted-foreground">
            Mostrando {(page - 1) * data.pageSize + 1} a Math.min(page * data.pageSize, data.totalCount) de {data.totalCount} registros
          </p>
          <div className="flex gap-1">
            <Button
              variant="outline"
              size="sm"
              className="h-8 w-8 p-0"
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="h-8 w-8 p-0"
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
