import { useState } from 'react'
import type { CuttingOrderDto, CuttingOrderItemDto } from '../types'
import type { SewingDeliveryItemInput } from '../hooks/useRegisterSewingDelivery'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']
const SEWING_PRICE: Record<string, number> = { P: 5.6, M: 5.6, G: 5.6, G1: 6.3, GG: 5.6 }

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

type Step = 'pieces' | 'defects' | 'confirm'

type PerRoll<T> = Record<string, T>

interface Props {
  order: CuttingOrderDto
  isLoading: boolean
  isPartial?: boolean
  onConfirm: (items: SewingDeliveryItemInput[]) => void
  onCancel: () => void
}

function initReceived(
  items: CuttingOrderItemDto[],
  sewingDeliveredPieces?: Record<string, number> | null,
  isPartial?: boolean,
): PerRoll<Record<string, number>> {
  return Object.fromEntries(
    items.map((item) => {
      const delivered = sewingDeliveredPieces ?? {}
      const numItems = items.length || 1
      return [
        item.id,
        Object.fromEntries(SIZES.map((s) => {
          const requested = item.requestedPieces[s] ?? 0
          if (isPartial) {
            const alreadyDeliveredForSize = Math.floor((delivered[s] ?? 0) / numItems)
            return [s, Math.max(0, requested - alreadyDeliveredForSize)]
          }
          return [s, requested]
        })),
      ]
    })
  )
}

function initDefects(items: CuttingOrderItemDto[]): PerRoll<Record<string, number>> {
  return Object.fromEntries(items.map((item) => [item.id, Object.fromEntries(SIZES.map((s) => [s, 0]))]))
}

function colorDot(hexCode: string | null) {
  if (!hexCode) return null
  return (
    <span
      className="inline-block w-3 h-3 rounded-full border border-border shrink-0"
      style={{ backgroundColor: hexCode }}
    />
  )
}

function itemGoodPieces(received: Record<string, number>, defects: Record<string, number>) {
  return Object.fromEntries(SIZES.map((s) => [s, Math.max(0, (received[s] ?? 0) - (defects[s] ?? 0))]))
}

export function SewingDeliveryForm({ order, isLoading, isPartial, onConfirm, onCancel }: Props) {
  const [step, setStep] = useState<Step>('pieces')
  const [received, setReceived] = useState<PerRoll<Record<string, number>>>(() =>
    isPartial ? initDefects(order.items) : initReceived(order.items, order.sewingDeliveredPieces, isPartial)
  )
  const [defects, setDefects] = useState<PerRoll<Record<string, number>>>(() => initDefects(order.items))

  const totalReceived = order.items.reduce(
    (sum, item) => sum + Object.values(received[item.id] ?? {}).reduce((a, b) => a + b, 0),
    0
  )

  function handleReceivedChange(itemId: string, size: string, val: string) {
    setReceived((prev) => ({
      ...prev,
      [itemId]: { ...prev[itemId], [size]: Math.max(0, parseInt(val) || 0) },
    }))
  }

  function handleDefectChange(itemId: string, size: string, val: string) {
    const max = received[itemId]?.[size] ?? 0
    setDefects((prev) => ({
      ...prev,
      [itemId]: { ...prev[itemId], [size]: Math.min(max, Math.max(0, parseInt(val) || 0)) },
    }))
  }

  function buildItems(): SewingDeliveryItemInput[] {
    return order.items.map((item) => ({
      fabricRollId: item.fabricRollId,
      goodPieces: itemGoodPieces(received[item.id] ?? {}, defects[item.id] ?? {}),
      defectivePieces: defects[item.id] ?? {},
    }))
  }

  // ── Step 1: peças recebidas ─────────────────────────────────────────────────
  if (step === 'pieces') {
    return (
      <div className="space-y-5">
        <div className="rounded-md bg-info/10 border border-info/20 px-3 py-2 text-sm text-info">
          Pedido #{order.orderNumber} — {order.items.length} bobina{order.items.length !== 1 ? 's' : ''}
        </div>

        <div className="space-y-4">
          {order.items.map((item) => {
            const itemReceived = received[item.id] ?? {}
            const subtotal = Object.values(itemReceived).reduce((a, b) => a + b, 0)
            return (
              <div key={item.id} className="rounded-md border border-border p-3 space-y-3 bg-card">
                <div className="flex items-center gap-2">
                  {colorDot(item.fabricColorHexCode)}
                  <span className="text-sm font-semibold text-foreground">
                    {item.fabricTypeName} {item.fabricTypeVariation} — {item.fabricColorName}
                  </span>
                </div>
                <div className="grid grid-cols-5 gap-2">
                  {SIZES.map((size) => {
                    const expected = item.requestedPieces[size] ?? 0
                    const alreadyDelivered = isPartial
                      ? Math.floor(((order.sewingDeliveredPieces ?? {})[size] ?? 0) / (order.items.length || 1))
                      : 0
                    const remaining = Math.max(0, expected - alreadyDelivered)
                    return (
                      <div key={size} className="text-center">
                        <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                        <input
                          type="number"
                          min="0"
                          value={itemReceived[size] ?? 0}
                          onChange={(e) => handleReceivedChange(item.id, size, e.target.value)}
                          className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                        />
                        <div className="text-xs text-muted-foreground mt-0.5">
                          {isPartial ? `rest: ${remaining}` : `esp: ${expected}`}
                        </div>
                      </div>
                    )
                  })}
                </div>
                {subtotal > 0 && (
                  <p className="text-xs text-muted-foreground text-right">
                    Subtotal: <span className="font-semibold text-foreground">{subtotal} peças</span>
                  </p>
                )}
              </div>
            )
          })}
        </div>

        {totalReceived > 0 && (
          <div className="text-sm border-t border-border pt-2 flex justify-between text-muted-foreground">
            <span>Total recebido: <span className="font-semibold text-foreground">{totalReceived} peças</span></span>
          </div>
        )}

        <div className="flex gap-2">
          <Button variant="outline" onClick={onCancel} className="flex-1">Cancelar</Button>
          <Button onClick={() => setStep('defects')} disabled={totalReceived === 0} className="flex-1">
            Informar defeitos →
          </Button>
        </div>
      </div>
    )
  }

  // ── Step 2: defeitos ────────────────────────────────────────────────────────
  if (step === 'defects') {
    const totalDefects = order.items.reduce(
      (sum, item) => sum + Object.values(defects[item.id] ?? {}).reduce((a, b) => a + b, 0),
      0
    )
    const totalGood = totalReceived - totalDefects

    return (
      <div className="space-y-5">
        <div className="rounded-md bg-warning/10 border border-warning/20 px-3 py-2 text-sm text-warning font-medium">
          Quantas camisetas vieram com defeito?
        </div>

        <div className="space-y-4">
          {order.items.map((item) => {
            const itemReceived = received[item.id] ?? {}
            const itemDefects = defects[item.id] ?? {}
            const hasAnyReceived = Object.values(itemReceived).some((v) => v > 0)
            if (!hasAnyReceived) return null
            return (
              <div key={item.id} className="rounded-md border border-border p-3 space-y-3 bg-card">
                <div className="flex items-center gap-2">
                  {colorDot(item.fabricColorHexCode)}
                  <span className="text-sm font-semibold text-foreground">
                    {item.fabricTypeName} {item.fabricTypeVariation} — {item.fabricColorName}
                  </span>
                </div>
                <div className="grid grid-cols-5 gap-2">
                  {SIZES.map((size) => {
                    const max = itemReceived[size] ?? 0
                    if (max === 0) return (
                      <div key={size} className="text-center opacity-30">
                        <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                        <input type="number" disabled value={0}
                          className="w-full rounded-md border border-border bg-muted px-2 py-2 text-sm text-center" />
                      </div>
                    )
                    return (
                      <div key={size} className="text-center">
                        <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                        <input
                          type="number"
                          min="0"
                          max={max}
                          value={itemDefects[size] ?? 0}
                          onChange={(e) => handleDefectChange(item.id, size, e.target.value)}
                          className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                        />
                        <div className="text-xs text-muted-foreground mt-0.5">max: {max}</div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>

        <div className="border-t border-border pt-3 space-y-1 text-sm">
          <div className="flex justify-between text-muted-foreground">
            <span>Peças boas</span>
            <span className="font-semibold text-success">{totalGood}</span>
          </div>
          {totalDefects > 0 && (
            <div className="flex justify-between text-muted-foreground">
              <span>Defeituosas</span>
              <span className="font-semibold text-danger">{totalDefects}</span>
            </div>
          )}
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('pieces')} className="flex-1">← Voltar</Button>
          <Button onClick={() => setStep('confirm')} disabled={totalGood === 0} className="flex-1">
            Revisar →
          </Button>
        </div>
      </div>
    )
  }

  // ── Step 3: confirmação ────────────────────────────────────────────────────
  const items = buildItems()
  const totalGoodAll = items.reduce((sum, i) => sum + Object.values(i.goodPieces).reduce((a, b) => a + b, 0), 0)
  const totalDefectAll = items.reduce((sum, i) => sum + Object.values(i.defectivePieces).reduce((a, b) => a + b, 0), 0)
  const sewingCostEst = items.reduce(
    (sum, i) => sum + Object.entries(i.goodPieces).reduce((a, [s, q]) => a + q * (SEWING_PRICE[s] ?? 5.6), 0),
    0
  )

  return (
    <div className="space-y-4">
      <div className="rounded-md border border-border bg-muted/50 p-4 space-y-4">
        <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
          Pedido #{order.orderNumber}
        </p>

        {order.items.map((item, idx) => {
          const input = items[idx]
          const goodEntries = Object.entries(input.goodPieces).filter(([, q]) => q > 0)
          const defectEntries = Object.entries(input.defectivePieces).filter(([, q]) => q > 0)
          const subtotalGood = goodEntries.reduce((a, [, q]) => a + q, 0)
          if (subtotalGood === 0 && defectEntries.length === 0) return null

          return (
            <div key={item.id} className="space-y-2">
              <div className="flex items-center gap-2">
                {colorDot(item.fabricColorHexCode)}
                <span className="text-sm font-semibold text-foreground">
                  {item.fabricTypeName} — {item.fabricColorName}
                </span>
              </div>
              {goodEntries.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {goodEntries.map(([size, qty]) => (
                    <Badge key={size} variant="success">
                      <span className="mr-1 text-xs">{size}</span><span>{qty}</span>
                    </Badge>
                  ))}
                  <span className="text-xs text-muted-foreground self-center ml-1">{subtotalGood} boas</span>
                </div>
              )}
              {defectEntries.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {defectEntries.map(([size, qty]) => (
                    <Badge key={size} variant="danger">
                      <span className="mr-1 text-xs">{size}</span><span>{qty}</span>
                    </Badge>
                  ))}
                  <span className="text-xs text-danger self-center ml-1">
                    {defectEntries.reduce((a, [, q]) => a + q, 0)} defeitos
                  </span>
                </div>
              )}
            </div>
          )
        })}

        <div className="border-t border-border pt-3 space-y-1 text-sm">
          <div className="flex justify-between text-foreground">
            <span>Total boas → estoque</span>
            <span className="font-bold text-success">{totalGoodAll} peças</span>
          </div>
          {totalDefectAll > 0 && (
            <div className="flex justify-between text-foreground">
              <span>Total defeituosas</span>
              <span className="font-bold text-danger">{totalDefectAll} peças</span>
            </div>
          )}
          <div className="flex justify-between text-muted-foreground">
            <span>Custo de costura (estimado)</span>
            <span className="font-semibold">R$ {fmt(sewingCostEst)}</span>
          </div>
          <p className="text-xs text-muted-foreground">* Valor exato calculado pelo servidor.</p>
        </div>
      </div>

      <div className="flex gap-2">
        <Button variant="outline" onClick={() => setStep('defects')} disabled={isLoading} className="flex-1">
          ← Editar
        </Button>
        <Button onClick={() => onConfirm(items)} disabled={isLoading} className="flex-1">
          {isLoading ? 'Registrando...' : isPartial ? 'Confirmar entrega parcial' : 'Confirmar entrega'}
        </Button>
      </div>
    </div>
  )
}
