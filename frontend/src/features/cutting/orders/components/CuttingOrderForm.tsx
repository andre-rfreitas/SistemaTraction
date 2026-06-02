import { useState, useMemo } from 'react'
import { useFabricRolls } from '@/features/fabric/rolls/hooks/useFabricRolls'
import type { FabricRollDto } from '@/features/fabric/rolls/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

const SIZES = ['P', 'M', 'G', 'G1', 'GG']

interface Props {
  onConfirm: (rollId: string, pieces: Record<string, number>, notes: string) => void
  isLoading: boolean
}

export function CuttingOrderForm({ onConfirm, isLoading }: Props) {
  const { data: rolls = [] } = useFabricRolls('Available')
  const [selectedRollId, setSelectedRollId] = useState('')
  const [pieces, setPieces] = useState<Record<string, number>>(
    Object.fromEntries(SIZES.map((s) => [s, 0]))
  )
  const [notes, setNotes] = useState('')
  const [showConfirm, setShowConfirm] = useState(false)

  const selectedRoll = useMemo(
    () => rolls.find((r) => r.id === selectedRollId) ?? null,
    [rolls, selectedRollId]
  )

  const totalPieces = Object.values(pieces).reduce((a, b) => a + b, 0)
  const hasAnyPiece = totalPieces > 0

  function handlePieceChange(size: string, value: string) {
    const num = Math.max(0, parseInt(value) || 0)
    setPieces((prev) => ({ ...prev, [size]: num }))
  }

  function handleSubmit() {
    if (!selectedRollId || !hasAnyPiece) return
    setShowConfirm(true)
  }

  if (showConfirm && selectedRoll) {
    return (
      <ConfirmationStep
        roll={selectedRoll}
        pieces={pieces}
        totalPieces={totalPieces}
        notes={notes}
        isLoading={isLoading}
        onBack={() => setShowConfirm(false)}
        onConfirm={() => onConfirm(selectedRollId, pieces, notes)}
      />
    )
  }

  return (
    <div className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-foreground mb-1">
          Bobina disponível
        </label>
        {rolls.length === 0 ? (
          <p className="text-sm text-warning bg-warning/10 border border-warning/20 rounded-md px-3 py-2">
            Nenhuma bobina disponível. Registre uma bobina primeiro.
          </p>
        ) : (
          <select
            value={selectedRollId}
            onChange={(e) => setSelectedRollId(e.target.value)}
            className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
          >
            <option value="">Selecione...</option>
            {rolls.map((r) => (
              <option key={r.id} value={r.id}>
                {r.fabricColorName} {r.fabricTypeName} {r.fabricTypeVariation} — {r.weightKg.toFixed(3)} kg
              </option>
            ))}
          </select>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Quantidades por tamanho
        </label>
        <div className="grid grid-cols-5 gap-2">
          {SIZES.map((size) => (
            <div key={size} className="text-center">
              <div className="text-xs font-semibold text-muted-foreground mb-1">{size}</div>
              <input
                type="number"
                min="0"
                value={pieces[size]}
                onChange={(e) => handlePieceChange(size, e.target.value)}
                className="w-full rounded-md border border-input bg-background px-2 py-2 text-sm text-foreground text-center shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              />
            </div>
          ))}
        </div>
        {hasAnyPiece && (
          <p className="text-xs text-muted-foreground mt-2 text-right">
            Total: <span className="font-semibold text-foreground">{totalPieces} peças</span>
          </p>
        )}
      </div>

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

      <Button
        onClick={handleSubmit}
        disabled={!selectedRollId || !hasAnyPiece}
        className="w-full"
      >
        Revisar pedido →
      </Button>
    </div>
  )
}

interface ConfirmationProps {
  roll: FabricRollDto
  pieces: Record<string, number>
  totalPieces: number
  notes: string
  isLoading: boolean
  onBack: () => void
  onConfirm: () => void
}

function ConfirmationStep({ roll, pieces, totalPieces, notes, isLoading, onBack, onConfirm }: ConfirmationProps) {
  const activePieces = Object.entries(pieces).filter(([, qty]) => qty > 0)

  return (
    <div className="space-y-4">
      <div className="rounded-md border border-border p-4 space-y-3 bg-muted/50">
        <div>
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1">Bobina</p>
          <p className="text-sm font-semibold text-foreground">
            {roll.fabricColorName} {roll.fabricTypeName} {roll.fabricTypeVariation} — {roll.weightKg.toFixed(3)} kg
          </p>
        </div>

        <div>
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-1">Peças</p>
          <div className="flex flex-wrap gap-2">
            {activePieces.map(([size, qty]) => (
              <Badge key={size} variant="outline">
                <span className="text-muted-foreground text-xs mr-1">{size}</span>
                <span>{qty}</span>
              </Badge>
            ))}
          </div>
          <p className="text-sm text-muted-foreground mt-1">Total: <span className="font-semibold text-foreground">{totalPieces} peças</span></p>
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
