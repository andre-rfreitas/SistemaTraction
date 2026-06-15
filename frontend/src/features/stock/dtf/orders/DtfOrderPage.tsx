import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { useDtfOrders } from './hooks/useDtfOrders'
import { useCreateDtfOrder } from './hooks/useCreateDtfOrder'
import { useUpdateDtfOrder } from './hooks/useUpdateDtfOrder'
import { useSendDtfOrder } from './hooks/useSendDtfOrder'
import { useReceiveDtfOrder } from './hooks/useReceiveDtfOrder'
import { useCancelDtfOrder } from './hooks/useCancelDtfOrder'
import { DtfOrderList } from './components/DtfOrderList'
import { DtfOrderForm } from './components/DtfOrderForm'
import type { DtfOrderDto } from './types'
import type { DtfOrderFormData } from './schemas/dtfOrderSchema'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'

interface DtfModelOption {
  id: string
  name: string
  sheetLabel: string
  stampsPerSheet: number
}

function useDtfModelOptions() {
  return useQuery({
    queryKey: ['dtf-models'],
    queryFn: async () => {
      const { data } = await api.get<DtfModelOption[]>('/dtf-models')
      return data
    },
  })
}

type ActiveDialog =
  | { type: 'create' }
  | { type: 'edit'; order: DtfOrderDto }
  | { type: 'send'; order: DtfOrderDto }
  | { type: 'receive'; order: DtfOrderDto }
  | { type: 'cancel'; order: DtfOrderDto }
  | null

export function DtfOrderPage() {
  const [dialog, setDialog] = useState<ActiveDialog>(null)

  const { data: orders = [], isLoading } = useDtfOrders()
  const { data: models = [] } = useDtfModelOptions()

  const createMutation = useCreateDtfOrder()
  const updateMutation = useUpdateDtfOrder()
  const sendMutation = useSendDtfOrder()
  const receiveMutation = useReceiveDtfOrder()
  const cancelMutation = useCancelDtfOrder()

  function handleSubmitCreate(data: DtfOrderFormData) {
    createMutation.mutate(data, { onSuccess: () => setDialog(null) })
  }

  function handleSubmitEdit(data: DtfOrderFormData) {
    if (dialog?.type !== 'edit') return
    updateMutation.mutate(
      { id: dialog.order.id, data },
      { onSuccess: () => setDialog(null) }
    )
  }

  function handleConfirmSend() {
    if (dialog?.type !== 'send') return
    sendMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  function handleConfirmReceive() {
    if (dialog?.type !== 'receive') return
    receiveMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  function handleConfirmCancel() {
    if (dialog?.type !== 'cancel') return
    cancelMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Pedidos DTF</h1>
        <Button onClick={() => setDialog({ type: 'create' })}>+ Novo pedido</Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="size-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        </div>
      ) : (
        <DtfOrderList
          orders={orders}
          onEdit={order => setDialog({ type: 'edit', order })}
          onSend={order => setDialog({ type: 'send', order })}
          onReceive={order => setDialog({ type: 'receive', order })}
          onCancel={order => setDialog({ type: 'cancel', order })}
        />
      )}

      {/* Dialog: criação */}
      <Dialog open={dialog?.type === 'create'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Novo Pedido DTF</DialogTitle>
          </DialogHeader>
          <DtfOrderForm
            models={models}
            isLoading={createMutation.isPending}
            onSubmit={handleSubmitCreate}
          />
        </DialogContent>
      </Dialog>

      {/* Dialog: edição */}
      <Dialog open={dialog?.type === 'edit'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              Editar Pedido DTF #{dialog?.type === 'edit' ? dialog.order.orderNumber : ''}
            </DialogTitle>
          </DialogHeader>
          {dialog?.type === 'edit' && (
            <DtfOrderForm
              models={models}
              defaultValues={dialog.order}
              isLoading={updateMutation.isPending}
              onSubmit={handleSubmitEdit}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Dialog: envio */}
      <Dialog open={dialog?.type === 'send'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Marcar como enviado?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            {dialog?.type === 'send' &&
              `Marcar pedido #${dialog.order.orderNumber} como enviado ao fornecedor?`}
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialog(null)} disabled={sendMutation.isPending}>
              Cancelar
            </Button>
            <Button onClick={handleConfirmSend} disabled={sendMutation.isPending}>
              {sendMutation.isPending ? 'Confirmando...' : 'Confirmar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Dialog: recebimento */}
      <Dialog open={dialog?.type === 'receive'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Marcar como recebido?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            {dialog?.type === 'receive' && (
              <>
                Marcar pedido #{dialog.order.orderNumber} como recebido? O estoque DTF será
                atualizado automaticamente com{' '}
                {dialog.order.items.reduce((acc, i) => acc + i.stampsTotal, 0)} estampas totais.
              </>
            )}
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialog(null)} disabled={receiveMutation.isPending}>
              Cancelar
            </Button>
            <Button onClick={handleConfirmReceive} disabled={receiveMutation.isPending}>
              {receiveMutation.isPending ? 'Confirmando...' : 'Confirmar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Dialog: cancelamento */}
      <Dialog open={dialog?.type === 'cancel'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Cancelar pedido?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            {dialog?.type === 'cancel' &&
              `Cancelar pedido #${dialog.order.orderNumber}? Esta ação não pode ser desfeita.`}
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialog(null)} disabled={cancelMutation.isPending}>
              Voltar
            </Button>
            <Button
              variant="destructive"
              onClick={handleConfirmCancel}
              disabled={cancelMutation.isPending}
            >
              {cancelMutation.isPending ? 'Cancelando...' : 'Cancelar pedido'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
