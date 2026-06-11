import { Pencil } from 'lucide-react'
import { useSupplyStockMovements } from '../hooks/useSupplyStockMovements'
import { Skeleton } from '@/components/ui/skeleton'
import type { SupplyStockMovementDto } from '../types'

interface Props {
  supplyStockItemId: string
  onEdit?: (movement: SupplyStockMovementDto) => void
}

const TYPE_LABEL: Record<string, string> = {
  Entrada: 'Entrada',
  Saida: 'Saída',
  Ajuste: 'Ajuste',
}

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

export function SupplyMovementHistory({ supplyStockItemId, onEdit }: Props) {
  const { data: movements = [], isLoading } = useSupplyStockMovements(supplyStockItemId)

  if (isLoading) return <Skeleton className="h-20 w-full" />

  if (movements.length === 0)
    return <p className="text-xs text-muted-foreground">Nenhuma movimentação registrada.</p>

  return (
    <div className="space-y-1">
      {movements.map((m) => {
        const date = new Date(m.occurredAt)
        const dateStr = date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: '2-digit' })
        return (
          <div key={m.id} className="flex items-start justify-between gap-2 py-1.5 border-b border-border last:border-0">
            <div className="flex-1 min-w-0 space-y-0.5">
              <div className="flex items-center gap-2">
                <span className={`text-sm font-semibold ${m.delta > 0 ? 'text-success' : 'text-danger'}`}>
                  {m.delta > 0 ? '+' : ''}{m.delta}
                </span>
                <span className="text-xs text-muted-foreground">{TYPE_LABEL[m.type]}</span>
                {m.reason && <span className="text-xs text-muted-foreground truncate">— {m.reason}</span>}
              </div>
              <div className="flex items-center gap-2 flex-wrap">
                {m.supplierName && (
                  <span className="text-xs text-muted-foreground">{m.supplierName}</span>
                )}
                {m.totalCost != null && (
                  <span className="text-xs font-medium text-foreground">R$ {fmt(m.totalCost)}</span>
                )}
                {m.unitPrice != null && (
                  <span className="text-xs text-muted-foreground">({fmt(m.unitPrice)}/un)</span>
                )}
              </div>
            </div>
            <div className="flex items-center gap-1 shrink-0">
              <span className="text-xs text-muted-foreground">{dateStr}</span>
              {onEdit && m.type === 'Entrada' && (
                <button
                  onClick={() => onEdit(m)}
                  className="p-1 rounded hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                  title="Editar"
                >
                  <Pencil className="size-3" />
                </button>
              )}
            </div>
          </div>
        )
      })}
    </div>
  )
}
