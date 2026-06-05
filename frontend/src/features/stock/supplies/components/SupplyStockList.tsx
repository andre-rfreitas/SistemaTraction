import { useSupplyStock } from '../hooks/useSupplyStock'
import type { SupplyStockItemDto } from '../types'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { Package } from 'lucide-react'

interface Props {
  onSelect: (item: SupplyStockItemDto) => void
}

export function SupplyStockList({ onSelect }: Props) {
  const { data: items = [], isLoading } = useSupplyStock()

  if (isLoading) return (
    <div className="space-y-2">
      <Skeleton className="h-14 w-full" />
      <Skeleton className="h-14 w-full" />
      <Skeleton className="h-14 w-full" />
    </div>
  )

  if (items.length === 0)
    return <EmptyState icon={Package} title="Nenhum insumo cadastrado." description="Cadastre tipos de insumo em Configurações → Tipos de Insumo." />

  return (
    <div className="space-y-2">
      {items.map((item) => (
        <button
          key={item.id}
          onClick={() => onSelect(item)}
          className="w-full flex items-center justify-between rounded-lg border border-border bg-card p-4 text-left hover:border-primary/40 hover:bg-primary/5 transition-colors group"
        >
          <div>
            <p className="text-sm font-semibold text-foreground">{item.name}</p>
            <p className="text-xs text-muted-foreground">{item.unit}</p>
          </div>
          <div className="text-right">
            <p className="text-2xl font-bold text-foreground">{item.quantity}</p>
            <p className="text-xs text-muted-foreground">{item.unit}</p>
          </div>
        </button>
      ))}
    </div>
  )
}
