import { useRef, useState } from 'react'
import type { ShirtStockGridDto } from '../types'
import { cn } from '@/lib/utils'

interface CellEdit {
  colorId: string
  size: string
}

interface Props {
  data: ShirtStockGridDto
  isSaving?: boolean
  onCellSave?: (colorId: string, colorName: string, size: string, newQty: number) => void
}

function cellClass(qty: number, threshold: number) {
  if (qty === 0) return 'bg-destructive/15 text-destructive font-semibold'
  if (qty <= threshold) return 'bg-warning/20 text-warning font-semibold'
  return ''
}

export function ShirtStockGrid({ data, isSaving, onCellSave }: Props) {
  const { sizes, rows, totalsPerSize, grandTotal } = data
  const [editing, setEditing] = useState<CellEdit | null>(null)
  const [draft, setDraft] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  function startEdit(colorId: string, size: string, currentQty: number) {
    if (!onCellSave || isSaving) return
    setEditing({ colorId, size })
    setDraft(String(currentQty))
    // input is rendered next tick
    setTimeout(() => inputRef.current?.select(), 0)
  }

  function commitEdit(colorId: string, colorName: string, size: string, currentQty: number) {
    const parsed = parseInt(draft, 10)
    if (!isNaN(parsed) && parsed >= 0 && parsed !== currentQty) {
      onCellSave?.(colorId, colorName, size, parsed)
    }
    setEditing(null)
  }

  function cancelEdit() {
    setEditing(null)
  }

  if (rows.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-card p-8 text-center text-sm text-muted-foreground">
        Nenhum item em estoque ainda. O estoque é alimentado automaticamente ao registrar entregas do costureiro.
      </div>
    )
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border bg-muted/50">
            <th className="py-2.5 px-4 text-left font-semibold text-foreground">Cor</th>
            {sizes.map((s) => (
              <th key={s} className="py-2.5 px-3 text-center font-semibold text-foreground w-16">{s}</th>
            ))}
            <th className="py-2.5 px-4 text-center font-semibold text-foreground">Total</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr key={row.colorId} className={cn('border-b border-border', i % 2 === 0 ? '' : 'bg-muted/20')}>
              <td className="py-2.5 px-4 font-medium text-foreground">{row.colorName}</td>
              {sizes.map((s) => {
                const qty = row.quantities[s] ?? 0
                const isEditing = editing?.colorId === row.colorId && editing?.size === s
                return (
                  <td
                    key={s}
                    className={cn(
                      'py-1 px-1 text-center tabular-nums',
                      !isEditing && cellClass(qty, data.alertThreshold),
                      onCellSave && !isSaving && 'group cursor-pointer hover:bg-primary/10 transition-colors',
                    )}
                    onClick={() => !isEditing && startEdit(row.colorId, s, qty)}
                  >
                    {isEditing ? (
                      <input
                        ref={inputRef}
                        type="number"
                        min="0"
                        value={draft}
                        onChange={(e) => setDraft(e.target.value)}
                        onBlur={() => commitEdit(row.colorId, row.colorName, s, qty)}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter') commitEdit(row.colorId, row.colorName, s, qty)
                          if (e.key === 'Escape') cancelEdit()
                        }}
                        className="w-14 rounded border border-primary bg-background px-1 py-0.5 text-center text-sm tabular-nums outline-none focus:ring-1 focus:ring-primary"
                        autoFocus
                      />
                    ) : (
                      <span className="inline-block w-full py-1.5">{qty}</span>
                    )}
                  </td>
                )
              })}
              <td className="py-2.5 px-4 text-center font-bold text-foreground tabular-nums">{row.total}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr className="border-t-2 border-border bg-muted/50 font-semibold">
            <td className="py-2.5 px-4 text-foreground">Total</td>
            {sizes.map((s) => (
              <td key={s} className="py-2.5 px-3 text-center tabular-nums text-foreground">
                {totalsPerSize[s] ?? 0}
              </td>
            ))}
            <td className="py-2.5 px-4 text-center font-bold text-foreground tabular-nums">{grandTotal}</td>
          </tr>
        </tfoot>
      </table>

      <div className="flex items-center gap-4 px-4 py-2 border-t border-border bg-muted/30 text-xs text-muted-foreground">
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-3 w-3 rounded bg-warning/40" />
          Alerta ≤ {data.alertThreshold} unidades
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-3 w-3 rounded bg-destructive/30" />
          Zerado
        </span>
        {onCellSave && (
          <span className="ml-auto opacity-60">Clique em uma quantidade para editar</span>
        )}
      </div>
    </div>
  )
}
