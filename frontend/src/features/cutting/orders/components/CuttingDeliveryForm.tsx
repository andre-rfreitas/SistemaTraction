import { useState } from 'react'
import type { CuttingOrderDto, CuttingOrderItemDto } from '../types'
import type { DeliveryItemInput } from '../hooks/useRegisterCuttingDelivery'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']
const PRICE_PER_PIECE = 1.0

interface Props {
  order: CuttingOrderDto
  isLoading: boolean
  onConfirm: (items: DeliveryItemInput[]) => void
  onCancel: () => void
}

type PerRollPieces = Record<string, Record<string, number>>

function initPerRoll(items: CuttingOrderItemDto[]): PerRollPieces {
  return Object.fromEntries(
    items.map((item) => [
      item.id,
      Object.fromEntries(SIZES.map((s) => [s, item.requestedPieces[s] ?? 0])),
    ])
  )
}

function aggregate(perRoll: PerRollPieces): Record<string, number> {
  const totals: Record<string, number> = Object.fromEntries(SIZES.map((s) => [s, 0]))
  for (const pieces of Object.values(perRoll)) {
    for (const size of SIZES) {
      totals[size] = (totals[size] ?? 0) + (pieces[size] ?? 0)
    }
  }
  return totals
}

function itemTotal(pieces: Record<string, number>) {
  return Object.values(pieces).reduce((a, b) => a + b, 0)
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

export function CuttingDeliveryForm({ order, isLoading, onConfirm, onCancel }: Props) {
  const [perRoll, setPerRoll] = useState<PerRollPieces>(() => initPerRoll(order.items))
  const [showConfirm, setShowConfirm] = useState(false)

  const totals = aggregate(perRoll)
  const totalPieces = Object.values(totals).reduce((a, b) => a + b, 0)
  const totalCost = totalPieces * PRICE_PER_PIECE

  function handleChange(itemId: string, size: string, value: string) {
    setPerRoll((prev) => ({
      ...prev,
      [itemId]: {
        ...prev[itemId],
        [size]: Math.max(0, parseInt(value) || 0),
      },
    }))
  }

  if (showConfirm) {
    return (
      <ConfirmStep
        order={order}
        perRoll={perRoll}
        totals={totals}
        totalPieces={totalPieces}
        totalCost={totalCost}
        isLoading={isLoading}
        onBack={() => setShowConfirm(false)}
        onConfirm={() =>
          onConfirm(
            order.items.map((item) => ({
              fabricRollId: item.fabricRollId,
              deliveredPieces: perRoll[item.id] ?? {},
            }))
          )
        }
      />
    )
  }

  return (
    <div className="space-y-5">
      <div className="rounded-md bg-info/10 border border-info/20 px-3 py-2 text-sm text-info">
        Pedido #{order.orderNumber} — {order.items.length} bobina{order.items.length !== 1 ? 's' : ''}
      </div>

      <div className="space-y-4">
        {order.items.map((item) => {
          const pieces = perRoll[item.id] ?? {}
          const subtotal = itemTotal(pieces)
          const requested = item.requestedPieces

          return (
            <div key={item.id} className="rounded-md border border-border p-3 space-y-3 bg-card">
              <div className="flex items-center gap-2">
                {colorDot(item.fabricColorHexCode)}
                <span className="text-sm font-semibold text-foreground">
                  {item.fabricTypeVariation} {item.fabricColorName}
                </span>
                <span className="text-xs text-muted-foreground ml-auto">
                  {item.fabricRollWeightKg.toFixed(3)} kg
                </span>
              </div>

              <div className="grid grid-cols-5 gap-2">
                {SIZES.map((size) => {
                  const req = requested[size] ?? 0
                  const curr = pieces[size] ?? 0
                  const diff = curr - req
                  return (
                    <div key={size} className="text-center">
                      <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                      <input
                        type="number"
                        min="0"
                        value={curr}
                        onChange={(e) => handleChange(item.id, size, e.target.value)}
                        className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                      />
                      {diff !== 0 && (
                        <div className={`text-xs mt-0.5 font-medium ${diff > 0 ? 'text-success' : 'text-danger'}`}>
                          {diff > 0 ? '+' : ''}{diff}
                        </div>
                      )}
                      <div className="text-xs text-muted-foreground mt-0.5">ped: {req}</div>
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

      {totalPieces > 0 && (
        <div className="flex justify-between text-sm border-t border-border pt-2">
          <span className="text-muted-foreground">
            Total: <span className="font-semibold text-foreground">{totalPieces} peças</span>
          </span>
          <span className="text-muted-foreground">
            Custo corte:{' '}
            <span className="font-semibold text-foreground">
              R$ {totalCost.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
            </span>
          </span>
        </div>
      )}

      <div className="flex gap-2">
        <Button variant="outline" onClick={onCancel} className="flex-1">Cancelar</Button>
        <Button
          onClick={() => setShowConfirm(true)}
          disabled={totalPieces === 0}
          className="flex-1"
        >
          Revisar entrega →
        </Button>
      </div>
    </div>
  )
}

interface ConfirmStepProps {
  order: CuttingOrderDto
  perRoll: PerRollPieces
  totals: Record<string, number>
  totalPieces: number
  totalCost: number
  isLoading: boolean
  onBack: () => void
  onConfirm: () => void
}

function ConfirmStep({ order, perRoll, totals, totalPieces, totalCost, isLoading, onBack, onConfirm }: ConfirmStepProps) {
  const requestedTotals = aggregate(
    Object.fromEntries(
      order.items.map((item) => [item.id, item.requestedPieces])
    )
  )
  const diffSizes = SIZES.filter((s) => (totals[s] ?? 0) !== (requestedTotals[s] ?? 0))

  return (
    <div className="space-y-4">
      <div className="rounded-md border border-border bg-muted/50 p-4 space-y-3">
        <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
          Pedido #{order.orderNumber}
        </p>

        {order.items.map((item) => {
          const pieces = perRoll[item.id] ?? {}
          const activePieces = SIZES.filter((s) => (pieces[s] ?? 0) > 0)
          const subtotal = itemTotal(pieces)
          if (subtotal === 0) return null
          return (
            <div key={item.id} className="space-y-1.5">
              <div className="flex items-center gap-2">
                {item.fabricColorHexCode && (
                  <span
                    className="inline-block w-3 h-3 rounded-full border border-border shrink-0"
                    style={{ backgroundColor: item.fabricColorHexCode }}
                  />
                )}
                <span className="text-sm font-semibold text-foreground">
                  {item.fabricTypeVariation} {item.fabricColorName}
                </span>
              </div>
              <div className="flex flex-wrap gap-1.5">
                {activePieces.map((size) => (
                  <Badge key={size} variant="outline">
                    <span className="text-muted-foreground text-xs mr-1">{size}</span>
                    <span>{pieces[size]}</span>
                  </Badge>
                ))}
                <span className="text-xs text-muted-foreground self-center ml-1">
                  {subtotal} peças
                </span>
              </div>
            </div>
          )
        })}

        {diffSizes.length > 0 && (
          <div className="text-xs text-warning bg-warning/10 border border-warning/20 rounded p-2">
            Diferença em relação ao pedido:{' '}
            {diffSizes.map((s) => {
              const diff = (totals[s] ?? 0) - (requestedTotals[s] ?? 0)
              return `${s}: ${diff > 0 ? '+' : ''}${diff}`
            }).join(', ')}
          </div>
        )}

        <div className="flex justify-between border-t border-border pt-2">
          <span className="text-sm font-medium text-foreground">
            Total: <span className="font-bold">{totalPieces} peças</span>
          </span>
          <span className="text-sm font-medium text-foreground">
            Custo: <span className="font-bold">
              R$ {totalCost.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
            </span>
          </span>
        </div>
      </div>

      <p className="text-xs text-muted-foreground">
        Ao confirmar: lançamento "Corte" será registrado e bobina(s) marcadas como consumidas.
      </p>

      <div className="flex gap-2">
        <Button variant="outline" onClick={onBack} disabled={isLoading} className="flex-1">
          Editar
        </Button>
        <Button onClick={onConfirm} disabled={isLoading} className="flex-1">
          {isLoading ? 'Registrando...' : 'Confirmar entrega'}
        </Button>
      </div>
    </div>
  )
}
