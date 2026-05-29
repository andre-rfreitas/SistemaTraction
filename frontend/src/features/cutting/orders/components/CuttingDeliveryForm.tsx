import { useState } from 'react'
import type { CuttingOrderDto } from '../types'
import { Button } from '@/components/ui/button'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']
const PRICE_PER_PIECE = 1.0

interface Props {
  order: CuttingOrderDto
  isLoading: boolean
  onConfirm: (deliveredPieces: Record<string, number>) => void
  onCancel: () => void
}

export function CuttingDeliveryForm({ order, isLoading, onConfirm, onCancel }: Props) {
  const [pieces, setPieces] = useState<Record<string, number>>(() => {
    const initial: Record<string, number> = {}
    for (const size of SIZES) {
      initial[size] = order.requestedPieces[size] ?? 0
    }
    return initial
  })
  const [showConfirm, setShowConfirm] = useState(false)

  const totalPieces = Object.values(pieces).reduce((a, b) => a + b, 0)
  const totalCost = totalPieces * PRICE_PER_PIECE

  function handleChange(size: string, value: string) {
    setPieces((prev) => ({ ...prev, [size]: Math.max(0, parseInt(value) || 0) }))
  }

  if (showConfirm) {
    const activePieces = Object.entries(pieces).filter(([, qty]) => qty > 0)
    const diffPieces = SIZES.filter(
      (s) => pieces[s] !== (order.requestedPieces[s] ?? 0)
    )

    return (
      <div className="space-y-4">
        <div className="rounded-md border border-neutral-200 bg-neutral-50 p-4 space-y-3">
          <div>
            <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-1">Pedido #{order.orderNumber}</p>
            <p className="text-sm font-semibold text-neutral-900">
              {order.fabricColorName} {order.fabricTypeName} {order.fabricTypeVariation}
            </p>
          </div>

          <div>
            <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-1">Peças entregues</p>
            <div className="flex flex-wrap gap-2">
              {activePieces.map(([size, qty]) => (
                <span key={size} className="inline-flex items-center gap-1 bg-white border border-neutral-300 rounded px-2 py-0.5 text-sm font-medium">
                  <span className="text-neutral-500 text-xs">{size}</span>
                  <span>{qty}</span>
                </span>
              ))}
            </div>
            <p className="text-sm text-neutral-600 mt-1">
              Total: <span className="font-semibold">{totalPieces} peças</span>
            </p>
          </div>

          {diffPieces.length > 0 && (
            <div className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded p-2">
              Diferença em relação ao pedido:{' '}
              {diffPieces.map((s) => {
                const diff = (pieces[s] ?? 0) - (order.requestedPieces[s] ?? 0)
                return `${s}: ${diff > 0 ? '+' : ''}${diff}`
              }).join(', ')}
            </div>
          )}

          <div className="flex justify-between border-t border-neutral-200 pt-2">
            <span className="text-sm font-medium text-neutral-700">Custo de corte:</span>
            <span className="text-sm font-bold text-neutral-900">
              R$ {totalCost.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
            </span>
          </div>
        </div>

        <p className="text-xs text-neutral-500">
          Ao confirmar: FinancialEntry "Corte" será lançado, pedido marcado como Entregue e bobina como Consumida.
        </p>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setShowConfirm(false)} disabled={isLoading} className="flex-1">
            Editar
          </Button>
          <Button onClick={() => onConfirm(pieces)} disabled={isLoading} className="flex-1">
            {isLoading ? 'Registrando...' : 'Confirmar entrega'}
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-5">
      <div className="rounded-md bg-blue-50 border border-blue-200 px-3 py-2 text-sm text-blue-800">
        Pedido #{order.orderNumber} — {order.fabricColorName} {order.fabricTypeName} {order.fabricTypeVariation}
      </div>

      <div>
        <label className="block text-sm font-medium text-neutral-700 mb-2">
          Peças entregues pelo cortador
        </label>
        <div className="grid grid-cols-5 gap-2">
          {SIZES.map((size) => {
            const requested = order.requestedPieces[size] ?? 0
            const current = pieces[size] ?? 0
            const diff = current - requested
            return (
              <div key={size} className="text-center">
                <div className="text-xs font-semibold text-neutral-500 mb-1">{size}</div>
                <input
                  type="number"
                  min="0"
                  value={pieces[size] ?? 0}
                  onChange={(e) => handleChange(size, e.target.value)}
                  className="w-full border border-neutral-300 rounded-md px-2 py-2 text-sm text-center focus:outline-none focus:ring-2 focus:ring-neutral-400"
                />
                {diff !== 0 && (
                  <div className={`text-xs mt-0.5 font-medium ${diff > 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {diff > 0 ? '+' : ''}{diff}
                  </div>
                )}
                <div className="text-xs text-neutral-400 mt-0.5">pedido: {requested}</div>
              </div>
            )
          })}
        </div>

        {totalPieces > 0 && (
          <div className="mt-3 flex justify-between text-sm border-t border-neutral-200 pt-2">
            <span className="text-neutral-600">
              Total: <span className="font-semibold text-neutral-900">{totalPieces} peças</span>
            </span>
            <span className="text-neutral-600">
              Custo corte:{' '}
              <span className="font-semibold text-neutral-900">
                R$ {totalCost.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
              </span>
            </span>
          </div>
        )}
      </div>

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
