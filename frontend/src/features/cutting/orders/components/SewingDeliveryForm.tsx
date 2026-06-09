import { useState } from 'react'
import type { CuttingOrderDto } from '../types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']
// Prices shown in real-time (mirrors AppConfig defaults; used only for UI preview — backend recalculates)
const SEWING_PRICE: Record<string, number> = { P: 5.6, M: 5.6, G: 5.6, G1: 6.3, GG: 5.6 }

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

type Step = 'pieces' | 'defects' | 'confirm'

interface Props {
  order: CuttingOrderDto
  isLoading: boolean
  onConfirm: (goodPieces: Record<string, number>, defectivePieces: Record<string, number>) => void
  onCancel: () => void
}

export function SewingDeliveryForm({ order, isLoading, onConfirm, onCancel }: Props) {
  const [step, setStep] = useState<Step>('pieces')
  const firstItem = order.items[0]

  // Step 1: total received from sewer (pre-fill from deliveredPieces if available)
  const [received, setReceived] = useState<Record<string, number>>(() => {
    const initial: Record<string, number> = {}
    for (const size of SIZES) {
      initial[size] = order.deliveredPieces?.[size] ?? order.requestedPieces[size] ?? 0
    }
    return initial
  })

  // Step 2: defective pieces per size
  const [defects, setDefects] = useState<Record<string, number>>(() =>
    Object.fromEntries(SIZES.map((s) => [s, 0])),
  )

  const totalReceived = Object.values(received).reduce((a, b) => a + b, 0)
  const totalDefects = Object.values(defects).reduce((a, b) => a + b, 0)

  const goodPieces = Object.fromEntries(
    SIZES.map((s) => [s, Math.max(0, (received[s] ?? 0) - (defects[s] ?? 0))]),
  )
  const totalGood = Object.values(goodPieces).reduce((a, b) => a + b, 0)

  const sewingCost = SIZES.reduce((acc, s) => acc + (goodPieces[s] ?? 0) * SEWING_PRICE[s], 0)

  function handleReceivedChange(size: string, val: string) {
    setReceived((prev) => ({ ...prev, [size]: Math.max(0, parseInt(val) || 0) }))
  }

  function handleDefectChange(size: string, val: string) {
    const max = received[size] ?? 0
    setDefects((prev) => ({ ...prev, [size]: Math.min(max, Math.max(0, parseInt(val) || 0)) }))
  }

  // ── Step 1: peças recebidas ─────────────────────────────────────────────────
  if (step === 'pieces') {
    return (
      <div className="space-y-5">
        <div className="rounded-md bg-info/10 border border-info/20 px-3 py-2 text-sm text-info">
          Pedido #{order.orderNumber} — {firstItem?.fabricColorName} {firstItem?.fabricTypeName}{' '}
          {firstItem?.fabricTypeVariation}
        </div>

        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            Peças recebidas do costureiro
          </label>
          <div className="grid grid-cols-5 gap-2">
            {SIZES.map((size) => {
              const requested = order.deliveredPieces?.[size] ?? order.requestedPieces[size] ?? 0
              return (
                <div key={size} className="text-center">
                  <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                  <input
                    type="number"
                    min="0"
                    value={received[size] ?? 0}
                    onChange={(e) => handleReceivedChange(size, e.target.value)}
                    className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  />
                  <div className="text-xs text-muted-foreground mt-0.5">esperado: {requested}</div>
                </div>
              )
            })}
          </div>

          {totalReceived > 0 && (
            <div className="mt-3 flex justify-between text-sm border-t border-border pt-2">
              <span className="text-muted-foreground">
                Total:{' '}
                <span className="font-semibold text-foreground">{totalReceived} peças</span>
              </span>
              <span className="text-muted-foreground text-xs self-center">
                Custo estimado: R$ {fmt(sewingCost)}
              </span>
            </div>
          )}
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={onCancel} className="flex-1">
            Cancelar
          </Button>
          <Button
            onClick={() => setStep('defects')}
            disabled={totalReceived === 0}
            className="flex-1"
          >
            Informar defeitos →
          </Button>
        </div>
      </div>
    )
  }

  // ── Step 2: defeitos ────────────────────────────────────────────────────────
  if (step === 'defects') {
    return (
      <div className="space-y-5">
        <div className="rounded-md bg-warning/10 border border-warning/20 px-3 py-2 text-sm text-warning font-medium">
          Quantas camisetas vieram com defeito?
        </div>

        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            Peças defeituosas por tamanho
          </label>
          <div className="grid grid-cols-5 gap-2">
            {SIZES.map((size) => {
              const max = received[size] ?? 0
              if (max === 0) {
                return (
                  <div key={size} className="text-center opacity-30">
                    <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                    <input
                      type="number"
                      disabled
                      value={0}
                      className="w-full rounded-md border border-border bg-muted px-2 py-2 text-sm text-center"
                    />
                  </div>
                )
              }
              return (
                <div key={size} className="text-center">
                  <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
                  <input
                    type="number"
                    min="0"
                    max={max}
                    value={defects[size] ?? 0}
                    onChange={(e) => handleDefectChange(size, e.target.value)}
                    className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  />
                  <div className="text-xs text-muted-foreground mt-0.5">max: {max}</div>
                </div>
              )
            })}
          </div>

          <div className="mt-3 border-t border-border pt-3 space-y-1 text-sm">
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
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('pieces')} className="flex-1">
            ← Voltar
          </Button>
          <Button
            onClick={() => setStep('confirm')}
            disabled={totalGood === 0}
            className="flex-1"
          >
            Revisar →
          </Button>
        </div>
      </div>
    )
  }

  // ── Step 3: confirmação ────────────────────────────────────────────────────
  const activeGood = SIZES.filter((s) => (goodPieces[s] ?? 0) > 0)
  const activeDefects = SIZES.filter((s) => (defects[s] ?? 0) > 0)

  return (
    <div className="space-y-4">
      <div className="rounded-md border border-border bg-muted/50 p-4 space-y-4">
        <div>
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1">
            Pedido #{order.orderNumber}
          </p>
          <p className="text-sm font-semibold text-foreground">
            {firstItem?.fabricColorName} {firstItem?.fabricTypeName} {firstItem?.fabricTypeVariation}
          </p>
        </div>

        <div>
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1.5">
            Peças boas → estoque
          </p>
          <div className="flex flex-wrap gap-2">
            {activeGood.map((size) => (
              <Badge key={size} variant="success">
                <span className="mr-1 text-xs">{size}</span>
                <span>{goodPieces[size]}</span>
              </Badge>
            ))}
          </div>
          <p className="text-sm text-muted-foreground mt-1.5">
            Total: <span className="font-semibold text-foreground">{totalGood} peças</span>
          </p>
        </div>

        {activeDefects.length > 0 && (
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1.5">
              Peças defeituosas
            </p>
            <div className="flex flex-wrap gap-2">
              {activeDefects.map((size) => (
                <Badge key={size} variant="danger">
                  <span className="mr-1 text-xs">{size}</span>
                  <span>{defects[size]}</span>
                </Badge>
              ))}
            </div>
            <p className="text-xs text-warning mt-1">
              Não entram no estoque. Custo será lançado em Defeitos.
            </p>
          </div>
        )}

        <div className="border-t border-border pt-3 space-y-1 text-sm">
          <div className="flex justify-between text-foreground">
            <span>Custo de costura (estimado)</span>
            <span className="font-semibold">R$ {fmt(sewingCost)}</span>
          </div>
          <p className="text-xs text-muted-foreground">
            * Valores exatos calculados pelo servidor com preços atuais de AppConfig.
          </p>
        </div>
      </div>

      <p className="text-xs text-muted-foreground">
        Ao confirmar:{' '}
        <span className="font-medium text-foreground">
          {totalGood} peças boas adicionadas ao estoque
        </span>
        {totalDefects > 0 && (
          <>
            {' '}• lançamento financeiro de Defeitos
          </>
        )}{' '}
        • lançamento de Costura.
      </p>

      <div className="flex gap-2">
        <Button variant="outline" onClick={() => setStep('defects')} disabled={isLoading} className="flex-1">
          ← Editar
        </Button>
        <Button
          onClick={() => onConfirm(goodPieces, defects)}
          disabled={isLoading}
          className="flex-1"
        >
          {isLoading ? 'Registrando...' : 'Confirmar entrega'}
        </Button>
      </div>
    </div>
  )
}
