import { useState } from 'react'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useCuttingOrders } from '@/features/cutting/orders/hooks/useCuttingOrders'
import { useRegisterSewingDelivery } from '@/features/cutting/orders/hooks/useRegisterSewingDelivery'
import { SewingDeliveryForm } from '@/features/cutting/orders/components/SewingDeliveryForm'
import { useSewers } from '../sewers/hooks/useSewers'
import type { CuttingOrderDto, RegisterSewingDeliveryResult } from '@/features/cutting/orders/types'

type SewingStep = 'form' | 'done'

export function SewingOrdersPage() {
  const { data: orders = [], isLoading } = useCuttingOrders('Delivered')
  const { data: sewers = [], isLoading: sewersLoading } = useSewers()
  const registerSewing = useRegisterSewingDelivery()

  const [open, setOpen] = useState(false)
  const [selectedOrder, setSelectedOrder] = useState<CuttingOrderDto | null>(null)
  const [step, setStep] = useState<SewingStep>('form')
  const [result, setResult] = useState<RegisterSewingDeliveryResult | null>(null)

  const hasActiveSewer = sewers.length > 0

  function handleOpen(order: CuttingOrderDto) {
    setSelectedOrder(order)
    setStep('form')
    setResult(null)
    setOpen(true)
  }

  function handleClose() {
    setOpen(false)
    setSelectedOrder(null)
    setResult(null)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Pedidos de Costura"
        description="Registre a entrega das costureiras para cada pedido cortado."
      />

      {!sewersLoading && !hasActiveSewer && (
        <div className="rounded-lg border border-warning/40 bg-warning/10 p-4 text-sm text-warning">
          Nenhuma costureira cadastrada. Cadastre uma costureira na aba <strong>Costureiras</strong> antes de registrar entregas.
        </div>
      )}

      {isLoading ? (
        <p className="text-muted-foreground text-sm">Carregando...</p>
      ) : orders.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-8 text-center">
          <p className="text-muted-foreground text-sm">
            Nenhum pedido aguardando entrega do costureiro.
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            Pedidos aparecem aqui após a entrega do cortador ser registrada.
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {orders.map((order) => {
            const deliveredPcs = order.deliveredPieces
              ? Object.values(order.deliveredPieces).reduce((a, b) => a + b, 0)
              : 0
            return (
              <div key={order.id} className="rounded-lg border border-border bg-card p-4">
                <div className="flex items-start justify-between gap-2">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-foreground">Pedido #{order.orderNumber}</span>
                      <Badge variant="warning">Aguardando costura</Badge>
                    </div>
                    <div className="flex flex-wrap gap-2 text-xs text-muted-foreground">
                      {order.items.map((item) => (
                        <div key={item.id} className="flex items-center gap-1">
                          {item.fabricColorHexCode && (
                            <span
                              className="inline-block w-2.5 h-2.5 rounded-full border border-border"
                              style={{ backgroundColor: item.fabricColorHexCode }}
                            />
                          )}
                          <span>{item.fabricTypeName} {item.fabricColorName}</span>
                        </div>
                      ))}
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {deliveredPcs} peças cortadas
                    </p>
                  </div>
                  <Button
                    size="sm"
                    onClick={() => handleOpen(order)}
                    disabled={!hasActiveSewer}
                  >
                    Registrar entrega
                  </Button>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {step === 'form'
                ? `Entrega do costureiro — Pedido #${selectedOrder?.orderNumber}`
                : `Pedido #${selectedOrder?.orderNumber} — Em estoque ✓`}
            </DialogTitle>
          </DialogHeader>

          {step === 'form' && selectedOrder && (
            <SewingDeliveryForm
              order={selectedOrder}
              isLoading={registerSewing.isPending}
              onConfirm={(items) =>
                registerSewing.mutate(
                  { orderId: selectedOrder.id, items },
                  { onSuccess: (r) => { setResult(r); setStep('done') } }
                )
              }
              onCancel={handleClose}
            />
          )}

          {step === 'done' && result && (
            <SewingDone result={result} onClose={handleClose} />
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}

function SewingDone({
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
          <div key={idx} className="space-y-1.5">
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

      <Button onClick={onClose} className="w-full">Fechar</Button>
    </div>
  )
}
