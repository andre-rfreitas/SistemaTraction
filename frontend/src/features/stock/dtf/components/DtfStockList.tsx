import { useDtfStock } from '../hooks/useDtfStock'
import type { DtfStockItemDto } from '../types'
import { LOW_STOCK_THRESHOLD } from '../types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useDtfModels } from '@/features/settings/dtf/hooks/useDtfModels'

interface Props {
  onSelect: (item: DtfStockItemDto | { dtfModelId: string; modelName: string; sheetLabel: string }) => void
}

export function DtfStockList({ onSelect }: Props) {
  const { data: stockItems, isLoading: loadingStock } = useDtfStock()
  const { data: allModels, isLoading: loadingModels } = useDtfModels()

  if (loadingStock || loadingModels)
    return <p className="text-neutral-500 text-sm">Carregando estoque...</p>

  if (!allModels?.length)
    return <p className="text-neutral-500 text-sm">Nenhum modelo DTF cadastrado.</p>

  const stockMap = new Map(stockItems?.map((s) => [s.dtfModelId, s]))

  return (
    <div className="space-y-3">
      {allModels.map((model) => {
        const stock = stockMap.get(model.id)
        const qty = stock?.currentQuantity ?? 0
        const isLow = qty <= LOW_STOCK_THRESHOLD
        const hasStock = !!stock

        return (
          <div
            key={model.id}
            className="border rounded-lg p-4 bg-white flex items-center justify-between gap-4"
          >
            <div className="flex items-center gap-3 min-w-0">
              <Badge variant="outline" className="shrink-0 font-mono text-xs">
                {model.sheetLabel}
              </Badge>
              <div className="min-w-0">
                <h3 className="font-semibold text-neutral-900 truncate">
                  {model.name}
                </h3>
                <p className="text-sm text-neutral-500">
                  {model.stampsPerSheet} estampas/folha
                </p>
              </div>
            </div>

            <div className="flex items-center gap-3 shrink-0">
              <div className="text-right">
                <p className={`text-2xl font-bold tabular-nums ${isLow ? 'text-red-600' : 'text-neutral-900'}`}>
                  {qty}
                </p>
                <p className="text-xs text-neutral-400">folhas</p>
                {isLow && hasStock && (
                  <p className="text-xs text-red-500 font-medium">⚠ estoque baixo</p>
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
