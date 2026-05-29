import type { CuttingOrderDto } from '../types'
import { useCuttingOrders } from '../hooks/useCuttingOrders'
import { Button } from '@/components/ui/button'

const STATUS_LABEL: Record<string, string> = {
  Draft: 'Rascunho',
  SentToCutter: 'Enviado ao cortador',
  Delivered: 'Entregue',
}

const STATUS_COLOR: Record<string, string> = {
  Draft: 'bg-neutral-100 text-neutral-600',
  SentToCutter: 'bg-blue-100 text-blue-800',
  Delivered: 'bg-green-100 text-green-800',
}

interface Props {
  onRegisterDelivery: (order: CuttingOrderDto) => void
}

export function CuttingOrderList({ onRegisterDelivery }: Props) {
  const { data: orders = [], isLoading } = useCuttingOrders()

  if (isLoading) return <p className="text-sm text-neutral-500">Carregando...</p>

  if (orders.length === 0)
    return <p className="text-sm text-neutral-500">Nenhum pedido de corte registrado.</p>

  return (
    <div className="space-y-2">
      {orders.map((o) => {
        const activePieces = Object.entries(o.requestedPieces).filter(([, qty]) => qty > 0)
        return (
          <div key={o.id} className="border border-neutral-200 rounded-lg p-3 bg-white">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="font-bold text-neutral-900 text-sm">#{o.orderNumber}</span>
                  <span className="text-sm text-neutral-700">
                    {o.fabricColorName} {o.fabricTypeName} {o.fabricTypeVariation}
                  </span>
                  <span className="text-xs text-neutral-400">— {o.fabricRollWeightKg.toFixed(3)} kg</span>
                </div>
                <div className="flex flex-wrap gap-1.5 mt-1.5">
                  {activePieces.map(([size, qty]) => (
                    <span key={size} className="text-xs bg-neutral-100 text-neutral-700 rounded px-1.5 py-0.5 font-medium">
                      {qty} {size}
                    </span>
                  ))}
                  <span className="text-xs text-neutral-500 self-center">= {o.totalPieces} peças</span>
                </div>
                {o.notes && <p className="text-xs text-neutral-500 mt-1 italic">{o.notes}</p>}
              </div>

              <div className="shrink-0 flex flex-col items-end gap-2">
                <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_COLOR[o.status] ?? 'bg-neutral-100'}`}>
                  {STATUS_LABEL[o.status] ?? o.status}
                </span>
                <p className="text-xs text-neutral-400">{new Date(o.createdAt).toLocaleDateString('pt-BR')}</p>
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
              </div>
            </div>
          </div>
        )
      })}
    </div>
  )
}
