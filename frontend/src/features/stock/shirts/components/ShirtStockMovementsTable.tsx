import { useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useShirtStockMovements } from '../hooks/useShirtStockMovements'
import { Button } from '@/components/ui/button'
import type { ShirtType } from '../types'

const ORIGIN_BADGE: Record<string, string> = {
  Manual:     'bg-primary/10 text-primary',
  Costureiro: 'bg-success/10 text-success',
  Separação:  'bg-warning/10 text-warning',
}

interface Props {
  shirtType: ShirtType
}

export function ShirtStockMovementsTable({ shirtType }: Props) {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useShirtStockMovements(shirtType, page, 20)

  if (isLoading) return <p className="text-sm text-muted-foreground">Carregando histórico...</p>
  if (!data || data.totalCount === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        Nenhuma movimentação registrada ainda.
      </p>
    )
  }

  const totalPages = Math.ceil(data.totalCount / data.pageSize)

  return (
    <div className="space-y-3">
      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border bg-muted/50">
              <th className="py-2.5 px-3 text-left font-semibold text-foreground">Data</th>
              <th className="py-2.5 px-3 text-left font-semibold text-foreground">Cor</th>
              <th className="py-2.5 px-3 text-center font-semibold text-foreground">Tam.</th>
              <th className="py-2.5 px-3 text-center font-semibold text-foreground">Delta</th>
              <th className="py-2.5 px-3 text-left font-semibold text-foreground">Motivo</th>
              <th className="py-2.5 px-3 text-center font-semibold text-foreground">Origem</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((m, i) => (
              <tr key={m.id} className={i % 2 === 0 ? '' : 'bg-muted/20'}>
                <td className="py-2 px-3 text-muted-foreground tabular-nums whitespace-nowrap">
                  {new Date(m.date).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })}
                </td>
                <td className="py-2 px-3 font-medium text-foreground">{m.fabricColorName}</td>
                <td className="py-2 px-3 text-center font-mono text-foreground">{m.size}</td>
                <td className={`py-2 px-3 text-center font-bold tabular-nums ${m.delta > 0 ? 'text-success' : 'text-destructive'}`}>
                  {m.delta > 0 ? `+${m.delta}` : m.delta}
                </td>
                <td className="py-2 px-3 text-muted-foreground max-w-[200px] truncate">{m.reason}</td>
                <td className="py-2 px-3 text-center">
                  <span className={`inline-block rounded px-2 py-0.5 text-xs font-medium ${ORIGIN_BADGE[m.origin] ?? 'bg-muted text-muted-foreground'}`}>
                    {m.origin}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>{data.totalCount} movimentação{data.totalCount !== 1 ? 'ões' : ''}</span>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="tabular-nums">{page} / {totalPages}</span>
            <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
