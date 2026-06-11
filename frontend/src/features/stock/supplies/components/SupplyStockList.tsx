import { Plus, MoreHorizontal } from 'lucide-react'
import { useSupplyStock } from '../hooks/useSupplyStock'
import type { SupplyStockItemDto } from '../types'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { Button } from '@/components/ui/button'
import { Package } from 'lucide-react'

interface Props {
  onEntry: (item: SupplyStockItemDto) => void
  onMovement: (item: SupplyStockItemDto) => void
}

export function SupplyStockList({ onEntry, onMovement }: Props) {
  const { data: items = [], isLoading } = useSupplyStock()

  if (isLoading) return (
    <div className="space-y-2">
      <Skeleton className="h-16 w-full" />
      <Skeleton className="h-16 w-full" />
      <Skeleton className="h-16 w-full" />
    </div>
  )

  if (items.length === 0)
    return <EmptyState icon={Package} title="Nenhum insumo cadastrado." description="Cadastre tipos de insumo em Configurações → Tipos de Insumo." />

  return (
    <div className="space-y-2">
      {items.map((item) => (
        <div
          key={item.id}
          className="flex items-center gap-3 rounded-lg border border-border bg-card p-4"
        >
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-foreground">{item.name}</p>
            <p className="text-xs text-muted-foreground">
              {item.unit}
              {item.pricePerUnit != null && (
                <span className="ml-2">
                  · R$ {item.pricePerUnit.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}/un
                </span>
              )}
            </p>
          </div>
          <div className="text-right shrink-0">
            <p className="text-2xl font-bold text-foreground">{item.quantity}</p>
            <p className="text-xs text-muted-foreground">{item.unit}</p>
          </div>
          <div className="flex gap-1.5 shrink-0">
            <Button size="sm" onClick={() => onEntry(item)}>
              <Plus className="size-3.5 mr-1" />
              Entrada
            </Button>
            <Button size="sm" variant="outline" onClick={() => onMovement(item)}>
              <MoreHorizontal className="size-3.5" />
            </Button>
          </div>
        </div>
      ))}
    </div>
  )
}
