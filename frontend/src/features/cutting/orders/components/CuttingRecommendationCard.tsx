import { Lightbulb, TrendingUp, Package, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { CuttingRecommendationDto } from '../types'

interface Props {
  recommendation: CuttingRecommendationDto | undefined
  isLoading: boolean
  onApply: (pieces: Record<string, number>) => void
}

export function CuttingRecommendationCard({ recommendation, isLoading, onApply }: Props) {
  if (isLoading) {
    return (
      <div className="rounded-md border border-border bg-muted/30 p-4 flex items-center gap-3 text-sm text-muted-foreground">
        <Loader2 className="h-4 w-4 animate-spin shrink-0" />
        Calculando recomendação...
      </div>
    )
  }

  if (!recommendation) return null

  const { recommendedPieces, demandBySize, currentStockBySize, daysUsed, basedOnOrders, hasSufficientHistory, colorName } = recommendation
  const totalRecommended = Object.values(recommendedPieces).reduce((a, b) => a + b, 0)
  const sizes = Object.keys(recommendedPieces)

  if (!hasSufficientHistory) {
    return (
      <div className="rounded-md border border-border bg-muted/30 p-4 space-y-1">
        <div className="flex items-center gap-2 text-sm font-medium text-foreground">
          <Lightbulb className="h-4 w-4 text-muted-foreground" />
          Recomendação de corte
        </div>
        <p className="text-xs text-muted-foreground">
          Sem histórico de listas de separação confirmadas para a cor <strong>{colorName}</strong> nos últimos {daysUsed} dias.
          A recomendação estará disponível após as primeiras separações serem confirmadas.
        </p>
      </div>
    )
  }

  const hasAnyRecommendation = totalRecommended > 0

  return (
    <div className="rounded-md border border-primary/30 bg-primary/5 p-4 space-y-3">
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center gap-2 text-sm font-medium text-foreground">
          <Lightbulb className="h-4 w-4 text-primary" />
          Recomendação de corte
        </div>
        <span className="text-xs text-muted-foreground">
          Baseado em <strong>{basedOnOrders}</strong> pedido{basedOnOrders !== 1 ? 's' : ''} nos últimos <strong>{daysUsed}</strong> dias
        </span>
      </div>

      <div className="grid grid-cols-5 gap-2">
        {sizes.map((size) => {
          const recommended = recommendedPieces[size] ?? 0
          const demand = demandBySize[size] ?? 0
          const stock = currentStockBySize[size] ?? 0
          return (
            <div key={size} className="text-center space-y-1">
              <div className="text-xs font-semibold text-muted-foreground">{size}</div>
              <div className={`text-lg font-bold ${recommended > 0 ? 'text-primary' : 'text-muted-foreground'}`}>
                {recommended}
              </div>
              <div className="flex flex-col gap-0.5">
                <span className="text-[10px] text-muted-foreground leading-tight flex items-center justify-center gap-0.5">
                  <TrendingUp className="h-2.5 w-2.5" />{demand} demanda
                </span>
                <span className="text-[10px] text-muted-foreground leading-tight flex items-center justify-center gap-0.5">
                  <Package className="h-2.5 w-2.5" />{stock} estoque
                </span>
              </div>
            </div>
          )
        })}
      </div>

      {hasAnyRecommendation ? (
        <div className="flex items-center justify-between pt-1 border-t border-primary/20">
          <p className="text-xs text-muted-foreground">
            Total recomendado: <span className="font-semibold text-foreground">{totalRecommended} peças</span>
          </p>
          <Button size="sm" variant="outline" onClick={() => onApply(recommendedPieces)} className="h-7 text-xs border-primary/40 text-primary hover:bg-primary/10">
            Usar recomendação
          </Button>
        </div>
      ) : (
        <p className="text-xs text-muted-foreground pt-1 border-t border-primary/20">
          Estoque atual já cobre a demanda histórica — nenhum corte necessário por enquanto.
        </p>
      )}
    </div>
  )
}
