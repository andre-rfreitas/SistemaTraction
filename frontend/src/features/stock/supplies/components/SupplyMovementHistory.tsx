import { useSupplyStockMovements } from '../hooks/useSupplyStockMovements'
import { Skeleton } from '@/components/ui/skeleton'

interface Props {
  supplyStockItemId: string
}

const TYPE_LABEL: Record<string, string> = {
  Entrada: 'Entrada',
  Saida: 'Saída',
  Ajuste: 'Ajuste',
}

export function SupplyMovementHistory({ supplyStockItemId }: Props) {
  const { data: movements = [], isLoading } = useSupplyStockMovements(supplyStockItemId)

  if (isLoading) return <Skeleton className="h-20 w-full" />

  if (movements.length === 0)
    return <p className="text-xs text-muted-foreground">Nenhuma movimentação registrada.</p>

  return (
    <div className="space-y-1">
      {movements.map((m) => (
        <div key={m.id} className="flex items-center justify-between text-sm py-1 border-b border-border last:border-0">
          <div>
            <span className={`font-medium ${m.delta > 0 ? 'text-success' : 'text-danger'}`}>
              {m.delta > 0 ? '+' : ''}{m.delta}
            </span>
            {' '}
            <span className="text-muted-foreground text-xs">{TYPE_LABEL[m.type]}</span>
            {m.reason && <span className="text-muted-foreground text-xs"> — {m.reason}</span>}
          </div>
          <span className="text-xs text-muted-foreground">
            {new Date(m.createdAt).toLocaleDateString('pt-BR')}
          </span>
        </div>
      ))}
    </div>
  )
}
