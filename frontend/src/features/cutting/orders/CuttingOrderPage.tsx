import { useState } from 'react'
import { CuttingOrderList } from './components/CuttingOrderList'
import { CuttingOrderForm } from './components/CuttingOrderForm'
import { CuttingDeliveryForm } from './components/CuttingDeliveryForm'
import { WhatsAppMessageReview } from './components/WhatsAppMessageReview'
import { WhatsAppSewerReview } from './components/WhatsAppSewerReview'
import { useCreateCuttingOrder } from './hooks/useCreateCuttingOrder'
import { useSendCuttingOrder } from './hooks/useSendCuttingOrder'
import { useRegisterCuttingDelivery } from './hooks/useRegisterCuttingDelivery'
import type { CreateCuttingOrderResult, CuttingOrderDto, RegisterCuttingDeliveryResult } from './types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

type OrderStep = 'form' | 'whatsapp'
type DeliveryStep = 'delivery-form' | 'delivery-whatsapp'

export function CuttingOrderPage() {
  // New order flow
  const [orderOpen, setOrderOpen] = useState(false)
  const [orderStep, setOrderStep] = useState<OrderStep>('form')
  const [orderResult, setOrderResult] = useState<CreateCuttingOrderResult | null>(null)

  // Delivery flow
  const [deliveryOpen, setDeliveryOpen] = useState(false)
  const [deliveryStep, setDeliveryStep] = useState<DeliveryStep>('delivery-form')
  const [selectedOrder, setSelectedOrder] = useState<CuttingOrderDto | null>(null)
  const [deliveryResult, setDeliveryResult] = useState<RegisterCuttingDeliveryResult | null>(null)

  const createOrder = useCreateCuttingOrder()
  const sendOrder = useSendCuttingOrder()
  const registerDelivery = useRegisterCuttingDelivery()

  function handleOrderClose() {
    setOrderOpen(false)
    setOrderStep('form')
    setOrderResult(null)
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
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-xl font-bold text-neutral-900">Pedidos de Corte</h2>
          <p className="text-sm text-neutral-500">
            Envie bobinas ao cortador e registre as entregas.
          </p>
        </div>
        <Button onClick={() => { setOrderOpen(true); setOrderStep('form') }}>+ Novo pedido</Button>
      </div>

      <CuttingOrderList onRegisterDelivery={handleDeliveryOpen} />

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
              onConfirm={(rollId, pieces, notes) =>
                createOrder.mutate(
                  { fabricRollId: rollId, requestedPieces: pieces, notes: notes || undefined },
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

      {/* Dialog: registrar entrega */}
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
              onConfirm={(deliveredPieces) =>
                registerDelivery.mutate(
                  { orderId: selectedOrder.id, deliveredPieces },
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
