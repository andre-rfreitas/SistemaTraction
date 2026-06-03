import { useDtfStock } from '../hooks/useDtfStock'
import type { DtfStockItemDto } from '../types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { Boxes } from 'lucide-react'
import { useDtfModels } from '@/features/settings/dtf/hooks/useDtfModels'
import { useAppConfigs } from '@/features/settings/config/hooks/useAppConfigs'

interface Props {
  onSelect: (
    item:
      | DtfStockItemDto
      | { dtfModelId: string; modelName: string; sheetLabel: string; stampsPerSheet: number }
  ) => void
}

export function DtfStockList({ onSelect }: Props) {
  const { data: stockItems, isLoading: loadingStock } = useDtfStock()
  const { data: allModels, isLoading: loadingModels } = useDtfModels()
  const { data: configs } = useAppConfigs()

  const threshold = Number(
    configs?.find((c) => c.key === 'dtf_stock_alert_threshold')?.value ?? 100
  )

  if (loadingStock || loadingModels)
    return (
      <div className="space-y-3">
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
      </div>
    )

  if (!allModels?.length)
    return (
      <EmptyState
        icon={Boxes}
        title="Nenhum modelo DTF cadastrado"
        description="Cadastre modelos DTF em Configurações para acompanhar o estoque."
      />
    )

  const stockMap = new Map(stockItems?.map((s) => [s.dtfModelId, s]))

  return (
    <div className="space-y-3">
      {allModels.map((model) => {
        const stock = stockMap.get(model.id)
        const qty = stock?.currentQuantity ?? 0
        const isLow = qty <= threshold
        const hasStock = !!stock

        return (
          <div
            key={model.id}
            className="border border-border rounded-lg p-4 bg-card flex items-center justify-between gap-4"
          >
            <div className="flex items-center gap-3 min-w-0">
              <Badge variant="outline" className="shrink-0 font-mono text-xs">
                {model.sheetLabel}
              </Badge>
              <div className="min-w-0">
                <h3 className="font-semibold text-foreground truncate">
                  {model.name}
                </h3>
                <p className="text-sm text-muted-foreground">
                  {model.stampsPerSheet} estampas/folha
                </p>
              </div>
            </div>

            <div className="flex items-center gap-3 shrink-0">
              <div className="text-right">
                <p className={`text-2xl font-bold tabular-nums ${isLow ? 'text-danger' : 'text-foreground'}`}>
                  {qty}
                </p>
                <p className="text-xs text-muted-foreground">estampas</p>
                <p className="text-xs text-muted-foreground">
                  ≈ {Math.floor(qty / model.stampsPerSheet)} folhas
                </p>
                {isLow && hasStock && (
                  <p className="text-xs text-warning font-medium">⚠ estoque baixo</p>
                )}
              </div>
              <Button
                size="sm"
                variant="outline"
                onClick={() =>
                  onSelect(
                    stock ?? {
                      dtfModelId: model.id,
                      modelName: model.name,
                      sheetLabel: model.sheetLabel,
                      stampsPerSheet: model.stampsPerSheet,
                    }
                  )
                }
              >
                Movimentar
              </Button>
            </div>
          </div>
        )
      })}
    </div>
  )
}
