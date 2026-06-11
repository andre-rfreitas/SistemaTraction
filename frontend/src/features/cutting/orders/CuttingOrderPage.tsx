import { useState } from 'react'
import { CuttingOrderList } from './components/CuttingOrderList'
import { CuttingOrderForm } from './components/CuttingOrderForm'
import { CuttingOrderEditForm } from './components/CuttingOrderEditForm'
import { CuttingDeliveryForm } from './components/CuttingDeliveryForm'
import { SewingDeliveryForm } from './components/SewingDeliveryForm'
import { WhatsAppMessageReview } from './components/WhatsAppMessageReview'
import { WhatsAppSewerReview } from './components/WhatsAppSewerReview'
import { RecommendationAccuracyHistory } from './components/RecommendationAccuracyHistory'
import { useCreateCuttingOrder } from './hooks/useCreateCuttingOrder'
import { useUpdateCuttingOrder } from './hooks/useUpdateCuttingOrder'
import { useCancelCuttingOrder } from './hooks/useCancelCuttingOrder'
import { useSendCuttingOrder } from './hooks/useSendCuttingOrder'
import { useRegisterCuttingDelivery } from './hooks/useRegisterCuttingDelivery'
import { useRegisterSewingDelivery } from './hooks/useRegisterSewingDelivery'
import type {
  CreateCuttingOrderResult,
  CuttingOrderDto,
  RegisterCuttingDeliveryResult,
  RegisterSewingDeliveryResult,
} from './types'
import type { RecommendationSnapshot, CuttingOrderItemInput } from './components/CuttingOrderForm'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { PageHeader } from '@/components/ui/page-header'

type OrderStep = 'form' | 'whatsapp'
type DeliveryStep = 'delivery-form' | 'delivery-whatsapp'
type SewingStep = 'sewing-form' | 'sewing-done'

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

  // Sewing delivery flow
  const [sewingOpen, setSewingOpen] = useState(false)
  const [sewingStep, setSewingStep] = useState<SewingStep>('sewing-form')
  const [sewingOrder, setSewingOrder] = useState<CuttingOrderDto | null>(null)
  const [sewingResult, setSewingResult] = useState<RegisterSewingDeliveryResult | null>(null)

  const createOrder = useCreateCuttingOrder()
  const updateOrder = useUpdateCuttingOrder()
  const cancelOrderMutation = useCancelCuttingOrder()
  const sendOrder = useSendCuttingOrder()
  const registerDelivery = useRegisterCuttingDelivery()
  const registerSewing = useRegisterSewingDelivery()

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

  function handleSewingOpen(order: CuttingOrderDto) {
    setSewingOrder(order)
    setSewingStep('sewing-form')
    setSewingResult(null)
    setSewingOpen(true)
  }

  function handleSewingClose() {
    setSewingOpen(false)
    setSewingOrder(null)
    setSewingResult(null)
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
        onRegisterSewingDelivery={handleSewingOpen}
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
              {cancelOrder?.status === 'SentToCutter'
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

      {/* Dialog: entrega do costureiro */}
      <Dialog open={sewingOpen} onOpenChange={(v) => { if (!v) handleSewingClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {sewingStep === 'sewing-form'
                ? `Entrega do costureiro — Pedido #${sewingOrder?.orderNumber}`
                : `Pedido #${sewingOrder?.orderNumber} — Em estoque ✓`}
            </DialogTitle>
          </DialogHeader>

          {sewingStep === 'sewing-form' && sewingOrder && (
            <SewingDeliveryForm
              order={sewingOrder}
              isLoading={registerSewing.isPending}
              onConfirm={(items) =>
                registerSewing.mutate(
                  { orderId: sewingOrder.id, items },
                  { onSuccess: (r) => { setSewingResult(r); setSewingStep('sewing-done') } }
                )
              }
              onCancel={handleSewingClose}
            />
          )}

          {sewingStep === 'sewing-done' && sewingResult && (
            <SewingDeliveryDone result={sewingResult} onClose={handleSewingClose} />
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}

function SewingDeliveryDone({
  result,
  onClose,
}: {
  result: RegisterSewingDeliveryResult
  onClose: () => void
}) {
  const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

  return (
    <div className="space-y-4">
      <div className="rounded-md bg-success/10 border border-success/20 p-4 space-y-2">
        <p className="font-semibold text-success text-sm">✓ Entrega registrada com sucesso!</p>
        <div className="grid grid-cols-2 gap-2 text-sm text-success">
          <div>
            <span className="text-xs text-success/80 block">Peças em estoque</span>
            <span className="font-bold text-lg">{result.totalGoodPieces}</span>
          </div>
          <div>
            <span className="text-xs text-success/80 block">Custo costura</span>
            <span className="font-bold">R$ {fmt(result.sewingCostTotal)}</span>
          </div>
        </div>
      </div>

      {result.items.map((item, idx) => {
        const goodEntries = Object.entries(item.goodPieces).filter(([, q]) => q > 0)
        const defectEntries = Object.entries(item.defectivePieces).filter(([, q]) => q > 0)
        if (goodEntries.length === 0 && defectEntries.length === 0) return null
        return (
          <div key={idx} className="space-y-2">
            <div className="flex items-center gap-2">
              {item.fabricColorHexCode && (
                <span
                  className="inline-block w-3 h-3 rounded-full border border-border shrink-0"
                  style={{ backgroundColor: item.fabricColorHexCode }}
                />
              )}
              <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
                {item.fabricTypeName} — {item.fabricColorName}
              </p>
            </div>
            {goodEntries.length > 0 && (
              <div className="flex flex-wrap gap-1.5">
                {goodEntries.map(([size, qty]) => (
                  <Badge key={size} variant="success">{qty} {size}</Badge>
                ))}
              </div>
            )}
            {defectEntries.length > 0 && (
              <div className="flex flex-wrap gap-1.5">
                {defectEntries.map(([size, qty]) => (
                  <Badge key={size} variant="warning">{qty} {size} (defeito)</Badge>
                ))}
              </div>
            )}
          </div>
        )
      })}

      {result.totalDefectivePieces > 0 && (
        <div className="rounded-md bg-warning/10 border border-warning/20 p-3 text-sm">
          <p className="font-medium text-warning">
            {result.totalDefectivePieces} peça(s) com defeito
          </p>
          <p className="text-xs text-warning mt-1">
            Custo de defeitos: <span className="font-semibold">R$ {fmt(result.defectCostTotal)}</span>
          </p>
        </div>
      )}

      <Button onClick={onClose} className="w-full">
        Fechar
      </Button>
    </div>
  )
}
