import { Scissors } from 'lucide-react'
import type { CuttingOrderDto } from '../types'
import { useCuttingOrders } from '../hooks/useCuttingOrders'
import { Button } from '@/components/ui/button'
import { Badge, type BadgeProps } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'

const STATUS_LABEL: Record<string, string> = {
  Draft: 'Rascunho',
  SentToCutter: 'Enviado ao cortador',
  Delivered: 'Entregue pelo cortador',
  SewingDelivered: 'Em estoque',
}

const STATUS_VARIANT: Record<string, BadgeProps['variant']> = {
  Draft: 'neutral',
  SentToCutter: 'info',
  Delivered: 'warning',
  SewingDelivered: 'success',
}

interface Props {
  onRegisterDelivery: (order: CuttingOrderDto) => void
  onRegisterSewingDelivery: (order: CuttingOrderDto) => void
  onEdit: (order: CuttingOrderDto) => void
  onCancel: (order: CuttingOrderDto) => void
}

export function CuttingOrderList({ onRegisterDelivery, onRegisterSewingDelivery, onEdit, onCancel }: Props) {
  const { data: orders = [], isLoading } = useCuttingOrders()

  if (isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-20 w-full rounded-lg" />
        <Skeleton className="h-20 w-full rounded-lg" />
        <Skeleton className="h-20 w-full rounded-lg" />
      </div>
    )
  }

  if (orders.length === 0) {
    return (
      <EmptyState
        icon={Scissors}
        title="Nenhum pedido de corte registrado."
      />
    )
  }

  return (
    <div className="space-y-2">
      {orders.map((o) => {
        const activePieces = Object.entries(o.requestedPieces).filter(([, qty]) => qty > 0)
        const canEdit = o.status === 'Draft'
        const canCancel = o.status === 'Draft' || o.status === 'SentToCutter'

        return (
          <div key={o.id} className="border border-border rounded-lg p-3 bg-card">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2 flex-wrap mb-1">
                  <span className="font-bold text-foreground text-sm">#{o.orderNumber}</span>
                  <Badge variant={STATUS_VARIANT[o.status] ?? 'neutral'} className="sm:hidden">
                    {STATUS_LABEL[o.status] ?? o.status}
                  </Badge>
                </div>

                {o.items.length === 1 ? (
                  <p className="text-sm text-foreground">
                    {o.items[0].fabricColorName} {o.items[0].fabricTypeName} {o.items[0].fabricTypeVariation}
                    <span className="text-xs text-muted-foreground ml-1">— {o.items[0].fabricRollWeightKg.toFixed(3)} kg</span>
                  </p>
                ) : (
                  <div className="space-y-0.5">
                    {o.items.map((item) => (
                      <p key={item.id} className="text-sm text-foreground">
                        {item.fabricColorName} {item.fabricTypeName} {item.fabricTypeVariation}
                        <span className="text-xs text-muted-foreground ml-1">— {item.fabricRollWeightKg.toFixed(3)} kg</span>
                      </p>
                    ))}
                  </div>
                )}

                <div className="flex flex-wrap gap-1.5 mt-1.5">
                  {activePieces.map(([size, qty]) => (
                    <Badge key={size} variant="neutral">
                      {qty} {size}
                    </Badge>
                  ))}
                  <span className="text-xs text-muted-foreground self-center">= {o.totalPieces} peças</span>
                </div>
                {o.notes && <p className="text-xs text-muted-foreground mt-1 italic">{o.notes}</p>}
              </div>

              <div className="shrink-0 flex flex-col items-end gap-2">
                <Badge variant={STATUS_VARIANT[o.status] ?? 'neutral'} className="hidden sm:inline-flex">
                  {STATUS_LABEL[o.status] ?? o.status}
                </Badge>
                <p className="text-xs text-muted-foreground">{new Date(o.createdAt).toLocaleDateString('pt-BR')}</p>

                {o.status === 'SentToCutter' && (
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => onRegisterDelivery(o)}
                    className="text-xs h-7 px-2"
                  >
                    Registrar entrega
                  </Button>
                )}
                {o.status === 'Delivered' && (
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => onRegisterSewingDelivery(o)}
                    className="text-xs h-7 px-2 border-warning/40 text-warning hover:bg-warning/10"
                  >
                    Entrega do costureiro
                  </Button>
                )}
                {canEdit && (
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onEdit(o)}
                    className="text-xs h-7 px-2"
                  >
                    Editar
                  </Button>
                )}
                {canCancel && (
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onCancel(o)}
                    className="text-xs h-7 px-2 text-destructive hover:text-destructive hover:bg-destructive/10"
                  >
                    Cancelar
                  </Button>
                )}
              </div>
            </div>
          </div>
        )
      })}
    </div>
  )
}
