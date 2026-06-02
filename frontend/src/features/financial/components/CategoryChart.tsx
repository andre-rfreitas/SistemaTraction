import type { CategoryTotalDto } from '../types'
import { categoryColor } from '../types'
import { formatBRL } from '../format'

interface Props {
  data: CategoryTotalDto[]
}

export function CategoryChart({ data }: Props) {
  if (data.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-card p-6 text-sm text-muted-foreground">
        Nenhuma despesa no período.
      </div>
    )
  }

  const max = Math.max(...data.map((d) => d.total))

  return (
    <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
      <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-3">
        Despesas por categoria
      </p>
      <div className="space-y-2">
        {data.map((d) => (
          <div key={d.category} className="flex items-center gap-3">
            <span className="w-20 shrink-0 text-sm text-muted-foreground">{d.category}</span>
            <div className="flex-1 h-5 bg-muted rounded overflow-hidden">
              <div
                className={`h-full ${categoryColor(d.category)} transition-all`}
                style={{ width: `${max > 0 ? (d.total / max) * 100 : 0}%` }}
              />
            </div>
            <span className="w-24 shrink-0 text-right text-sm font-medium tabular-nums text-foreground">
              {formatBRL(d.total)}
            </span>
          </div>
        ))}
      </div>
    </div>
  )
}
