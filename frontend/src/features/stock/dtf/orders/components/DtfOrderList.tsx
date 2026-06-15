import type { DtfOrderDto } from '../types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Rascunho',
  Sent: 'Enviado',
  Received: 'Recebido',
}

const STATUS_VARIANTS: Record<string, 'outline' | 'warning' | 'success'> = {
  Draft: 'outline',
  Sent: 'warning',
  Received: 'success',
}

interface Props {
  orders: DtfOrderDto[]
  onEdit: (order: DtfOrderDto) => void
  onSend: (order: DtfOrderDto) => void
  onReceive: (order: DtfOrderDto) => void
  onCancel: (order: DtfOrderDto) => void
}

export function DtfOrderList({ orders, onEdit, onSend, onReceive, onCancel }: Props) {
  if (orders.length === 0) {
    return (
      <div className="py-12 text-center text-sm text-muted-foreground">
        Nenhum pedido DTF encontrado.
      </div>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border text-left text-muted-foreground">
            <th className="pb-2 pr-4 font-medium">Nº</th>
            <th className="pb-2 pr-4 font-medium">Data</th>
            <th className="pb-2 pr-4 font-medium">Modelos</th>
            <th className="pb-2 pr-4 font-medium">Total de folhas</th>
            <th className="pb-2 pr-4 font-medium">Status</th>
            <th className="pb-2 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(order => {
            const totalSheets = order.items.reduce((acc, i) => acc + i.sheetQuantity, 0)
            const modelsSummary = order.items.map(i => i.modelName).join(', ')

            return (
              <tr key={order.id} className="border-b border-border/50 hover:bg-muted/30">
                <td className="py-3 pr-4 font-medium">#{order.orderNumber}</td>
                <td className="py-3 pr-4 text-muted-foreground">
                  {new Date(order.createdAt).toLocaleDateString('pt-BR')}
                </td>
                <td className="py-3 pr-4 max-w-[200px] truncate" title={modelsSummary}>
                  {modelsSummary || '—'}
                </td>
                <td className="py-3 pr-4">{totalSheets}</td>
                <td className="py-3 pr-4">
                  <Badge variant={STATUS_VARIANTS[order.status]}>
                    {STATUS_LABELS[order.status]}
                  </Badge>
                </td>
                <td className="py-3">
                  <div className="flex items-center gap-2">
                    {order.status === 'Draft' && (
                      <>
                        <Button size="sm" variant="outline" onClick={() => onEdit(order)}>
                          Editar
                        </Button>
                        <Button size="sm" variant="outline" onClick={() => onSend(order)}>
                          Marcar Enviado
                        </Button>
                        <Button size="sm" variant="outline" className="text-danger hover:text-danger" onClick={() => onCancel(order)}>
                          Cancelar
                        </Button>
                      </>
                    )}
                    {order.status === 'Sent' && (
                      <>
                        <Button size="sm" variant="outline" onClick={() => onReceive(order)}>
                          Marcar Recebido
                        </Button>
                        <Button size="sm" variant="outline" className="text-danger hover:text-danger" onClick={() => onCancel(order)}>
                          Cancelar
                        </Button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
