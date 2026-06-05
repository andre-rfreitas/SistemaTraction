import { useState } from 'react'
import { useFabricRolls } from '@/features/fabric/rolls/hooks/useFabricRolls'
import type { FabricRollDto } from '@/features/fabric/rolls/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useCuttingRecommendation } from '../hooks/useCuttingRecommendation'
import { CuttingRecommendationCard } from './CuttingRecommendationCard'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']

export interface RecommendationSnapshot {
  pieces: Record<string, number>
  days: number
  basedOnOrders: number
}

export interface CuttingOrderItemInput {
  rollId: string
  pieces: Record<string, number>
}

interface Props {
  onConfirm: (
    items: CuttingOrderItemInput[],
    notes: string,
    recommendation: RecommendationSnapshot | null
  ) => void
  isLoading: boolean
}

function emptyPieces() {
  return Object.fromEntries(SIZES.map((s) => [s, 0]))
}

export function CuttingOrderForm({ onConfirm, isLoading }: Props) {
  const { data: rolls = [] } = useFabricRolls('Available')
  const [items, setItems] = useState<CuttingOrderItemInput[]>([{ rollId: '', pieces: emptyPieces() }])
  const [notes, setNotes] = useState('')
  const [showConfirm, setShowConfirm] = useState(false)
  const [activeItemIndex, setActiveItemIndex] = useState(0)

  const usedRollIds = items.map((i) => i.rollId).filter(Boolean)
  const availableForItem = (idx: number) =>
    rolls.filter((r) => r.id === items[idx].rollId || !usedRollIds.includes(r.id))

  const totalPieces = items.reduce(
    (sum, item) => sum + Object.values(item.pieces).reduce((a, b) => a + b, 0),
    0
  )
  const allItemsValid = items.every(
    (item) => item.rollId && Object.values(item.pieces).reduce((a, b) => a + b, 0) > 0
  )

  function handleRollChange(idx: number, rollId: string) {
    setItems((prev) => prev.map((item, i) => (i === idx ? { ...item, rollId, pieces: emptyPieces() } : item)))
    setActiveItemIndex(idx)
  }

  function handlePieceChange(idx: number, size: string, value: string) {
    const num = Math.max(0, parseInt(value) || 0)
    setItems((prev) =>
      prev.map((item, i) => (i === idx ? { ...item, pieces: { ...item.pieces, [size]: num } } : item))
    )
  }

  function handleAddItem() {
    setItems((prev) => [...prev, { rollId: '', pieces: emptyPieces() }])
    setActiveItemIndex(items.length)
  }

  function handleRemoveItem(idx: number) {
    setItems((prev) => prev.filter((_, i) => i !== idx))
    setActiveItemIndex(Math.max(0, activeItemIndex - 1))
  }

  function handleSubmit() {
    if (!allItemsValid) return
    setShowConfirm(true)
  }

  if (showConfirm) {
    const rollMap = Object.fromEntries(rolls.map((r) => [r.id, r]))
    return (
      <ConfirmationStep
        items={items}
        rollMap={rollMap}
        totalPieces={totalPieces}
        notes={notes}
        isLoading={isLoading}
        onBack={() => setShowConfirm(false)}
        onConfirm={() => onConfirm(items, notes, null)}
      />
    )
  }

  return (
    <div className="space-y-5">
      {rolls.length === 0 ? (
        <p className="text-sm text-warning bg-warning/10 border border-warning/20 rounded-md px-3 py-2">
          Nenhuma bobina disponível. Registre uma bobina primeiro.
        </p>
      ) : (
        <div className="space-y-4">
          {items.map((item, idx) => (
            <BobinaItem
              key={idx}
              idx={idx}
              item={item}
              availableRolls={availableForItem(idx)}
              canRemove={items.length > 1}
              isActive={activeItemIndex === idx}
              onFocus={() => setActiveItemIndex(idx)}
              onRollChange={(rollId) => handleRollChange(idx, rollId)}
              onPieceChange={(size, value) => handlePieceChange(idx, size, value)}
              onRemove={() => handleRemoveItem(idx)}
            />
          ))}

          {rolls.length > usedRollIds.filter(Boolean).length && (
            <button
              type="button"
              onClick={handleAddItem}
              className="w-full text-sm text-primary border border-dashed border-primary/40 rounded-md py-2 hover:bg-primary/5 transition-colors"
            >
              + Adicionar outra bobina
            </button>
          )}
        </div>
      )}

      <div>
        <label className="block text-sm font-medium text-foreground mb-1">
          Observações (opcional)
        </label>
        <textarea
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          rows={2}
          placeholder="Ex: urgente, entregar até sexta..."
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground resize-none shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      {totalPieces > 0 && (
        <p className="text-xs text-muted-foreground text-right">
          Total: <span className="font-semibold text-foreground">{totalPieces} peças</span>
          {items.length > 1 && (
            <span className="ml-1">em {items.filter((i) => i.rollId).length} bobina(s)</span>
          )}
        </p>
      )}

      <Button
        onClick={handleSubmit}
        disabled={!allItemsValid}
        className="w-full"
      >
        Revisar pedido →
      </Button>
    </div>
  )
}

interface BobinaItemProps {
  idx: number
  item: CuttingOrderItemInput
  availableRolls: FabricRollDto[]
  canRemove: boolean
  isActive: boolean
  onFocus: () => void
  onRollChange: (rollId: string) => void
  onPieceChange: (size: string, value: string) => void
  onRemove: () => void
}

function BobinaItem({ idx, item, availableRolls, canRemove, onFocus, onRollChange, onPieceChange, onRemove }: BobinaItemProps) {
  const { data: recommendation, isLoading: isLoadingRec } = useCuttingRecommendation(
    item.rollId || null
  )
  const itemTotal = Object.values(item.pieces).reduce((a, b) => a + b, 0)

  function handleApplyRecommendation(recPieces: Record<string, number>) {
    SIZES.forEach((s) => onPieceChange(s, String(recPieces[s] ?? 0)))
  }

  return (
    <div
      className="rounded-md border border-border p-3 space-y-3 bg-card"
      onFocus={onFocus}
    >
      <div className="flex items-center justify-between gap-2">
        <label className="text-sm font-medium text-foreground">
          Bobina {idx + 1}
        </label>
        {canRemove && (
          <button
            type="button"
            onClick={onRemove}
            className="text-xs text-danger hover:text-danger/80 transition-colors"
          >
            Remover
          </button>
        )}
      </div>

      <select
        value={item.rollId}
        onChange={(e) => onRollChange(e.target.value)}
        className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
      >
        <option value="">Selecione uma bobina...</option>
        {availableRolls.map((r) => (
          <option key={r.id} value={r.id}>
            {r.fabricColorName} {r.fabricTypeName} {r.fabricTypeVariation} — {r.weightKg.toFixed(3)} kg
          </option>
        ))}
      </select>

      {item.rollId && (
        <CuttingRecommendationCard
          recommendation={recommendation}
          isLoading={isLoadingRec}
          onApply={handleApplyRecommendation}
        />
      )}

      <div className="grid grid-cols-5 gap-2">
        {SIZES.map((size) => (
          <div key={size} className="text-center">
            <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
            <input
              type="number"
              min="0"
              value={item.pieces[size]}
              onChange={(e) => onPieceChange(size, e.target.value)}
              className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            />
          </div>
        ))}
      </div>

      {itemTotal > 0 && (
        <p className="text-xs text-muted-foreground text-right">
          Subtotal: <span className="font-semibold text-foreground">{itemTotal} peças</span>
        </p>
      )}
    </div>
  )
}

interface ConfirmationProps {
  items: CuttingOrderItemInput[]
  rollMap: Record<string, FabricRollDto>
  totalPieces: number
  notes: string
  isLoading: boolean
  onBack: () => void
  onConfirm: () => void
}

function ConfirmationStep({ items, rollMap, totalPieces, notes, isLoading, onBack, onConfirm }: ConfirmationProps) {
  return (
    <div className="space-y-4">
      <div className="rounded-md border border-border p-4 space-y-4 bg-muted/50">
        {items.map((item, idx) => {
          const roll = rollMap[item.rollId]
          if (!roll) return null
          const activePieces = Object.entries(item.pieces).filter(([, qty]) => qty > 0)
          const itemTotal = activePieces.reduce((a, [, q]) => a + q, 0)
          return (
            <div key={idx} className={idx > 0 ? 'pt-3 border-t border-border' : ''}>
              <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1">
                Bobina {idx + 1}
              </p>
              <p className="text-sm font-semibold text-foreground mb-2">
                {roll.fabricColorName} {roll.fabricTypeName} {roll.fabricTypeVariation} — {roll.weightKg.toFixed(3)} kg
              </p>
              <div className="flex flex-wrap gap-2">
                {activePieces.map(([size, qty]) => (
                  <Badge key={size} variant="outline">
                    <span className="text-muted-foreground text-xs mr-1">{size}</span>
                    <span>{qty}</span>
                  </Badge>
                ))}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Subtotal: <span className="font-semibold text-foreground">{itemTotal} peças</span>
              </p>
            </div>
          )
        })}

        <div className="pt-2 border-t border-border">
          <p className="text-sm font-semibold text-foreground">
            Total: {totalPieces} peças em {items.length} bobina{items.length !== 1 ? 's' : ''}
          </p>
        </div>

        {notes && (
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-0.5">Observações</p>
            <p className="text-sm text-foreground">{notes}</p>
          </div>
        )}
      </div>

      <p className="text-xs text-muted-foreground">
        Ao confirmar, o pedido será criado e você poderá revisar a mensagem antes de enviar ao cortador.
      </p>

      <div className="flex gap-2">
        <Button variant="outline" onClick={onBack} disabled={isLoading} className="flex-1">
          Editar
        </Button>
        <Button onClick={onConfirm} disabled={isLoading} className="flex-1">
          {isLoading ? 'Criando...' : 'Confirmar pedido'}
        </Button>
      </div>
    </div>
  )
}
