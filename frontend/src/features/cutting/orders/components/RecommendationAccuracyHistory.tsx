import { History, ChevronDown, ChevronUp } from 'lucide-react'
import { useState } from 'react'
import { useCuttingRecommendationHistory } from '../hooks/useCuttingRecommendationHistory'
import { Badge } from '@/components/ui/badge'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']

function diffColor(recommended: number, actual: number | undefined): string {
  if (actual === undefined) return 'text-muted-foreground'
  const diff = Math.abs(recommended - actual)
  const pct = recommended > 0 ? diff / recommended : diff > 0 ? 1 : 0
  if (pct === 0) return 'text-success'
  if (pct <= 0.15) return 'text-warning'
  return 'text-destructive'
}

function diffLabel(recommended: number, actual: number): string {
  const diff = actual - recommended
  if (diff === 0) return '='
  return diff > 0 ? `+${diff}` : `${diff}`
}

export function RecommendationAccuracyHistory() {
  const { data = [], isLoading } = useCuttingRecommendationHistory(8)
  const [expanded, setExpanded] = useState(false)

  if (isLoading) return null
  if (data.length === 0) return null

  const visible = expanded ? data : data.slice(0, 3)

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 text-sm font-medium text-foreground">
        <History className="h-4 w-4 text-muted-foreground" />
        Histórico de precisão
      </div>

      <div className="space-y-2">
        {visible.map((item) => {
          const sizes = SIZES.filter(
            (s) => (item.recommendedPieces[s] ?? 0) > 0 || (item.requestedPieces[s] ?? 0) > 0
          )
          const hasDelivery = item.actualDeliveredPieces !== null

          return (
            <div key={item.cuttingOrderId} className="rounded-md border border-border bg-muted/30 p-3 space-y-2">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-foreground">Pedido #{item.orderNumber}</span>
                  <Badge variant="outline" className="text-xs">{item.fabricColorName}</Badge>
                </div>
                <span className="text-xs text-muted-foreground">
                  {new Date(item.createdAt).toLocaleDateString('pt-BR')}
                </span>
              </div>

              <div className="grid gap-1">
                <div className="grid grid-cols-[auto_1fr] gap-x-3 text-xs">
                  <span className="text-muted-foreground font-medium w-20">Tamanho</span>
                  <div className="grid grid-cols-3 gap-1 text-center">
                    <span className="text-muted-foreground">Recomendado</span>
                    <span className="text-muted-foreground">Cortado</span>
                    {hasDelivery && <span className="text-muted-foreground">Entregue</span>}
                  </div>
                </div>

                {sizes.map((size) => {
                  const rec = item.recommendedPieces[size] ?? 0
                  const cut = item.requestedPieces[size] ?? 0
                  const delivered = item.actualDeliveredPieces?.[size]
                  return (
                    <div key={size} className="grid grid-cols-[auto_1fr] gap-x-3 text-xs">
                      <span className="text-muted-foreground w-20 font-medium">{size}</span>
                      <div className="grid grid-cols-3 gap-1 text-center">
                        <span className="font-mono">{rec}</span>
                        <span className={`font-mono ${diffColor(rec, cut)}`}>
                          {cut}
                          {rec !== cut && (
                            <span className="ml-1 text-[10px]">({diffLabel(rec, cut)})</span>
                          )}
                        </span>
                        {hasDelivery && (
                          <span className={`font-mono ${diffColor(cut, delivered)}`}>
                            {delivered ?? '—'}
                          </span>
                        )}
                      </div>
                    </div>
                  )
                })}
              </div>

              <p className="text-[11px] text-muted-foreground">
                Baseado em {item.basedOnOrders} pedido{item.basedOnOrders !== 1 ? 's' : ''} — {item.daysUsed} dias
              </p>
            </div>
          )
        })}
      </div>

      {data.length > 3 && (
        <button
          onClick={() => setExpanded((v) => !v)}
          className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground transition-colors"
        >
          {expanded ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
          {expanded ? 'Ver menos' : `Ver mais ${data.length - 3} entradas`}
        </button>
      )}
    </div>
  )
}
