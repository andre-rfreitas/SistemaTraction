import type { ShirtStockGridDto } from '../types'
import { cn } from '@/lib/utils'

interface Props {
  data: ShirtStockGridDto
}

function cellClass(qty: number, threshold: number) {
  if (qty === 0) return 'bg-destructive/15 text-destructive font-semibold'
  if (qty <= threshold) return 'bg-warning/20 text-warning font-semibold'
  return ''
}

export function ShirtStockGrid({ data }: Props) {
  const { sizes, rows, totalsPerSize, grandTotal } = data

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
                return (
                  <td key={s} className={cn('py-2.5 px-3 text-center tabular-nums', cellClass(qty, data.alertThreshold))}>
                    {qty}
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
      </div>
    </div>
  )
}
