import { useState } from 'react'
import { CuttingOrderList } from './components/CuttingOrderList'
import { CuttingOrderForm } from './components/CuttingOrderForm'
import { WhatsAppMessageReview } from './components/WhatsAppMessageReview'
import { useCreateCuttingOrder } from './hooks/useCreateCuttingOrder'
import { useSendCuttingOrder } from './hooks/useSendCuttingOrder'
import type { CreateCuttingOrderResult } from './types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

type Step = 'form' | 'whatsapp'

export function CuttingOrderPage() {
  const [open, setOpen] = useState(false)
  const [step, setStep] = useState<Step>('form')
  const [orderResult, setOrderResult] = useState<CreateCuttingOrderResult | null>(null)

  const createOrder = useCreateCuttingOrder()
  const sendOrder = useSendCuttingOrder()

  function handleClose() {
    setOpen(false)
    setStep('form')
    setOrderResult(null)
  }

  async function handleFormConfirm(
    rollId: string,
    pieces: Record<string, number>,
    notes: string
  ) {
    createOrder.mutate(
      { fabricRollId: rollId, requestedPieces: pieces, notes: notes || undefined },
      {
        onSuccess: (result) => {
          setOrderResult(result)
          setStep('whatsapp')
        },
      }
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-xl font-bold text-neutral-900">Pedidos de Corte</h2>
          <p className="text-sm text-neutral-500">
            Envie bobinas ao cortador e acompanhe os pedidos.
          </p>
        </div>
        <Button onClick={() => { setOpen(true); setStep('form') }}>+ Novo pedido</Button>
      </div>

      <CuttingOrderList />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {step === 'form' ? 'Novo pedido de corte' : `Revisar mensagem — Pedido #${orderResult?.orderNumber}`}
            </DialogTitle>
          </DialogHeader>

          {step === 'form' && (
            <CuttingOrderForm
              onConfirm={handleFormConfirm}
              isLoading={createOrder.isPending}
            />
          )}

          {step === 'whatsapp' && orderResult && (
            <WhatsAppMessageReview
              result={orderResult}
              isSending={sendOrder.isPending}
              onConfirmSend={() =>
                sendOrder.mutate(orderResult.cuttingOrderId)
              }
              onDone={handleClose}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
