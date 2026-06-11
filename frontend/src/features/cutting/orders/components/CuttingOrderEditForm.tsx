import { useState } from 'react'
import type { CuttingOrderDto, UpdateCuttingOrderInput } from '../types'
import { Button } from '@/components/ui/button'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']

interface Props {
  order: CuttingOrderDto
  isLoading: boolean
  onConfirm: (input: UpdateCuttingOrderInput) => void
  onCancel: () => void
}

type PerRollPieces = Record<string, Record<string, number>>

function initPerRoll(order: CuttingOrderDto): PerRollPieces {
  return Object.fromEntries(
    order.items.map((item) => [
      item.fabricRollId,
      Object.fromEntries(SIZES.map((s) => [s, item.requestedPieces[s] ?? 0])),
    ])
  )
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

export function CuttingOrderEditForm({ order, isLoading, onConfirm, onCancel }: Props) {
  const [perRoll, setPerRoll] = useState<PerRollPieces>(() => initPerRoll(order))
  const [notes, setNotes] = useState(order.notes ?? '')

  const totalPieces = Object.values(perRoll).flatMap(Object.values).reduce((a, b) => a + b, 0)

  function handleChange(rollId: string, size: string, value: string) {
    setPerRoll((prev) => ({
      ...prev,
      [rollId]: {
        ...prev[rollId],
        [size]: Math.max(0, parseInt(value) || 0),
      },
    }))
  }

  function handleConfirm() {
    onConfirm({
      orderId: order.id,
      items: order.items.map((item) => ({
        fabricRollId: item.fabricRollId,
        requestedPieces: perRoll[item.fabricRollId] ?? {},
      })),
      notes: notes.trim() || undefined,
    })
  }

  return (
    <div className="space-y-5">
      <div className="rounded-md bg-info/10 border border-info/20 px-3 py-2 text-sm text-info">
        Pedido #{order.orderNumber} — editar rascunho
      </div>

      <div className="space-y-4">
        {order.items.map((item) => {
          const pieces = perRoll[item.fabricRollId] ?? {}
          const subtotal = Object.values(pieces).reduce((a, b) => a + b, 0)

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
                {SIZES.map((size) => (
                  <div key={size} className="text-center">
                    <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                    <input
                      type="number"
                      min="0"
                      value={pieces[size] ?? 0}
                      onChange={(e) => handleChange(item.fabricRollId, size, e.target.value)}
                      className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    />
                  </div>
                ))}
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

      <div className="space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">Observações (opcional)</label>
        <textarea
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          rows={2}
          placeholder="Notas para o cortador..."
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm resize-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      {totalPieces > 0 && (
        <p className="text-sm text-muted-foreground border-t border-border pt-2">
          Total: <span className="font-semibold text-foreground">{totalPieces} peças</span>
        </p>
      )}

      <div className="flex gap-2">
        <Button variant="outline" onClick={onCancel} disabled={isLoading} className="flex-1">
          Cancelar
        </Button>
        <Button
          onClick={handleConfirm}
          disabled={totalPieces === 0 || isLoading}
          className="flex-1"
        >
          {isLoading ? 'Salvando...' : 'Salvar alterações'}
        </Button>
      </div>
    </div>
  )
}
