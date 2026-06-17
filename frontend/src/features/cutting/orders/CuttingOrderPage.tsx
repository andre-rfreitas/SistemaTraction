import { useState } from 'react'
import { CuttingOrderList } from './components/CuttingOrderList'
import { CuttingOrderForm } from './components/CuttingOrderForm'
import { CuttingOrderEditForm } from './components/CuttingOrderEditForm'
import { CuttingDeliveryForm } from './components/CuttingDeliveryForm'
import { WhatsAppMessageReview } from './components/WhatsAppMessageReview'
import { WhatsAppSewerReview } from './components/WhatsAppSewerReview'
import { RecommendationAccuracyHistory } from './components/RecommendationAccuracyHistory'
import { useCreateCuttingOrder } from './hooks/useCreateCuttingOrder'
import { useUpdateCuttingOrder } from './hooks/useUpdateCuttingOrder'
import { useCancelCuttingOrder } from './hooks/useCancelCuttingOrder'
import { useSendCuttingOrder } from './hooks/useSendCuttingOrder'
import { useRegisterCuttingDelivery } from './hooks/useRegisterCuttingDelivery'
import type {
  CreateCuttingOrderResult,
  CuttingOrderDto,
  RegisterCuttingDeliveryResult,
} from './types'
import type { RecommendationSnapshot, CuttingOrderItemInput } from './components/CuttingOrderForm'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { PageHeader } from '@/components/ui/page-header'

type OrderStep = 'form' | 'whatsapp'
type DeliveryStep = 'delivery-form' | 'delivery-whatsapp'

export function CuttingOrderPage() {
  // New order flow
  const [orderOpen, setOrderOpen] = useState(false)
  const [orderStep, setOrderStep] = useState<OrderStep>('form')
  const [orderResult, setOrderResult] = useState<CreateCuttingOrderResult | null>(null)

  // Edit flow
  const [editOpen, setEditOpen] = useState(false)
  const [editOrder, setEditOrder] = useState<CuttingOrderDto | null>(null)

  // Cancel confirmation
  const [cancelOrder, setCancelOrder] = useState<CuttingOrderDto | null>(null)

  // Cutter delivery flow
  const [deliveryOpen, setDeliveryOpen] = useState(false)
  const [deliveryStep, setDeliveryStep] = useState<DeliveryStep>('delivery-form')
  const [selectedOrder, setSelectedOrder] = useState<CuttingOrderDto | null>(null)
  const [deliveryResult, setDeliveryResult] = useState<RegisterCuttingDeliveryResult | null>(null)

  const createOrder = useCreateCuttingOrder()
  const updateOrder = useUpdateCuttingOrder()
  const cancelOrderMutation = useCancelCuttingOrder()
  const sendOrder = useSendCuttingOrder()
  const registerDelivery = useRegisterCuttingDelivery()

  function handleOrderClose() {
    setOrderOpen(false)
    setOrderStep('form')
    setOrderResult(null)
  }

  function handleEditOpen(order: CuttingOrderDto) {
    setEditOrder(order)
    setEditOpen(true)
  }

  function handleEditClose() {
    setEditOpen(false)
    setEditOrder(null)
  }

  function handleCancelOpen(order: CuttingOrderDto) {
    setCancelOrder(order)
  }

  function handleCancelConfirm() {
    if (!cancelOrder) return
    cancelOrderMutation.mutate(cancelOrder.id, {
      onSuccess: () => setCancelOrder(null),
    })
  }

  function handleDeliveryOpen(order: CuttingOrderDto) {
    setSelectedOrder(order)
    setDeliveryStep('delivery-form')
    setDeliveryResult(null)
    setDeliveryOpen(true)
  }

  function handleDeliveryClose() {
    setDeliveryOpen(false)
    setSelectedOrder(null)
    setDeliveryResult(null)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Pedidos de Corte"
        description="Envie bobinas ao cortador e registre as entregas."
        actions={
          <Button onClick={() => { setOrderOpen(true); setOrderStep('form') }}>+ Novo pedido</Button>
        }
      />

      <CuttingOrderList
        onRegisterDelivery={handleDeliveryOpen}
        onEdit={handleEditOpen}
        onCancel={handleCancelOpen}
      />

      <RecommendationAccuracyHistory />

      {/* Dialog: novo pedido */}
      <Dialog open={orderOpen} onOpenChange={(v) => { if (!v) handleOrderClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {orderStep === 'form' ? 'Novo pedido de corte' : `Revisar mensagem — Pedido #${orderResult?.orderNumber}`}
            </DialogTitle>
          </DialogHeader>

          {orderStep === 'form' && (
            <CuttingOrderForm
              onConfirm={(items: CuttingOrderItemInput[], notes: string, recommendation: RecommendationSnapshot | null) =>
                createOrder.mutate(
                  {
                    items: items.map((i) => ({ fabricRollId: i.rollId, requestedPieces: i.pieces })),
                    notes: notes || undefined,
                    recommendedPieces: recommendation?.pieces ?? null,
                    recommendationDays: recommendation?.days ?? null,
                    recommendationBasedOnOrders: recommendation?.basedOnOrders ?? null,
                  },
                  { onSuccess: (r) => { setOrderResult(r); setOrderStep('whatsapp') } }
                )
              }
              isLoading={createOrder.isPending}
            />
          )}

          {orderStep === 'whatsapp' && orderResult && (
            <WhatsAppMessageReview
              result={orderResult}
              isSending={sendOrder.isPending}
              onConfirmSend={() => sendOrder.mutate(orderResult.cuttingOrderId)}
              onDone={handleOrderClose}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Dialog: editar pedido */}
      <Dialog open={editOpen} onOpenChange={(v) => { if (!v) handleEditClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Editar pedido #{editOrder?.orderNumber}</DialogTitle>
          </DialogHeader>
          {editOrder && (
            <CuttingOrderEditForm
              order={editOrder}
              isLoading={updateOrder.isPending}
              onConfirm={(input) =>
                updateOrder.mutate(input, { onSuccess: handleEditClose })
              }
              onCancel={handleEditClose}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Dialog: confirmar cancelamento */}
      <Dialog open={!!cancelOrder} onOpenChange={(v) => { if (!v) setCancelOrder(null) }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Cancelar pedido #{cancelOrder?.orderNumber}?</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">
              {cancelOrder?.status === 'Delivered'
                ? 'Este cancelamento estornará o lançamento financeiro de corte e reverterá as bobinas de tecido para disponível.'
                : cancelOrder?.status === 'SewingDelivered'
                ? 'Este cancelamento estornará os lançamentos de costura e defeitos, reverterá o estoque de camisetas e cancelará o pedido.'
                : cancelOrder?.status === 'SentToCutter'
                ? 'O pedido foi enviado ao cortador. As bobinas serão revertidas para disponível.'
                : 'O rascunho será removido permanentemente.'}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => setCancelOrder(null)}
                disabled={cancelOrderMutation.isPending}
                className="flex-1"
              >
                Voltar
              </Button>
              <Button
                variant="destructive"
                onClick={handleCancelConfirm}
                disabled={cancelOrderMutation.isPending}
                className="flex-1"
              >
                {cancelOrderMutation.isPending ? 'Cancelando...' : 'Confirmar cancelamento'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Dialog: registrar entrega do cortador */}
      <Dialog open={deliveryOpen} onOpenChange={(v) => { if (!v) handleDeliveryClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {deliveryStep === 'delivery-form'
                ? `Entrega do cortador — Pedido #${selectedOrder?.orderNumber}`
                : `Avisar costureiro — Pedido #${selectedOrder?.orderNumber}`}
            </DialogTitle>
          </DialogHeader>

          {deliveryStep === 'delivery-form' && selectedOrder && (
            <CuttingDeliveryForm
              order={selectedOrder}
              isLoading={registerDelivery.isPending}
              onConfirm={(items) =>
                registerDelivery.mutate(
                  { orderId: selectedOrder.id, items },
                  { onSuccess: (r) => { setDeliveryResult(r); setDeliveryStep('delivery-whatsapp') } }
                )
              }
              onCancel={handleDeliveryClose}
            />
          )}

          {deliveryStep === 'delivery-whatsapp' && deliveryResult && (
            <WhatsAppSewerReview result={deliveryResult} onDone={handleDeliveryClose} />
          )}
        </DialogContent>
      </Dialog>

    </div>
  )
}
