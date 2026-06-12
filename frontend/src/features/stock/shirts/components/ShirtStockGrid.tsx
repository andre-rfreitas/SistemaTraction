import type { ShirtStockGridDto } from '../types'
import { cn } from '@/lib/utils'

interface Props {
  data: ShirtStockGridDto
  editMode?: boolean
  draftQuantities?: Record<string, Record<string, number>>
  onQuantityChange?: (colorId: string, size: string, value: number) => void
}

function cellClass(qty: number, threshold: number) {
  if (qty === 0) return 'bg-destructive/15 text-destructive font-semibold'
  if (qty <= threshold) return 'bg-warning/20 text-warning font-semibold'
  return ''
}

export function ShirtStockGrid({ data, editMode, draftQuantities, onQuantityChange }: Props) {
  const { sizes, rows, totalsPerSize, grandTotal } = data

  if (rows.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-card p-8 text-center text-sm text-muted-foreground">
        Nenhum item em estoque ainda. O estoque é alimentado automaticamente ao registrar entregas do costureiro.
      </div>
    )
  }

  return (
    <div className={cn('overflow-x-auto rounded-lg border', editMode ? 'border-primary/40' : 'border-border')}>
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
          {rows.map((row, i) => {
            const draftRow = draftQuantities?.[row.colorId]
            return (
              <tr key={row.colorId} className={cn('border-b border-border', i % 2 === 0 ? '' : 'bg-muted/20')}>
                <td className="py-2.5 px-4 font-medium text-foreground">{row.colorName}</td>
                {sizes.map((s) => {
                  const originalQty = row.quantities[s] ?? 0
                  const draftQty = draftRow?.[s] ?? originalQty
                  const changed = editMode && draftQty !== originalQty

                  return (
                    <td
                      key={s}
                      className={cn(
                        'py-1 px-1 text-center tabular-nums',
                        !editMode && cellClass(originalQty, data.alertThreshold),
                        editMode && changed && 'bg-primary/10',
                      )}
                    >
                      {editMode ? (
                        <input
                          type="number"
                          min="0"
                          value={draftQty}
                          onChange={(e) =>
                            onQuantityChange?.(row.colorId, s, Math.max(0, parseInt(e.target.value) || 0))
                          }
                          className={cn(
                            'w-14 rounded border bg-background px-1 py-0.5 text-center text-sm tabular-nums outline-none focus:ring-1 focus:ring-primary',
                            changed ? 'border-primary' : 'border-input',
                          )}
                        />
                      ) : (
                        <span className="inline-block w-full py-1.5">{originalQty}</span>
                      )}
                    </td>
                  )
                })}
                <td className="py-2.5 px-4 text-center font-bold text-foreground tabular-nums">{row.total}</td>
              </tr>
            )
          })}
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
        {editMode && (
          <span className="ml-auto text-primary font-medium">Campos com borda azul foram alterados</span>
        )}
      </div>
    </div>
  )
}
