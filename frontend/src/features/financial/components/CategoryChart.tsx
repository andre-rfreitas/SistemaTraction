import type { CategoryTotalDto } from '../types'
import { categoryColor } from '../types'
import { formatBRL } from '../format'

interface Props {
  data: CategoryTotalDto[]
}

export function CategoryChart({ data }: Props) {
  if (data.length === 0) {
    return (
      <div className="rounded-lg border border-neutral-200 bg-white p-6 text-sm text-neutral-500">
        Nenhuma despesa no período.
      </div>
    )
  }

  const max = Math.max(...data.map((d) => d.total))

  return (
    <div className="rounded-lg border border-neutral-200 bg-white p-4">
      <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wide mb-3">
        Despesas por categoria
      </p>
      <div className="space-y-2">
        {data.map((d) => (
          <div key={d.category} className="flex items-center gap-3">
            <span className="w-20 shrink-0 text-sm text-neutral-700">{d.category}</span>
            <div className="flex-1 h-5 bg-neutral-100 rounded overflow-hidden">
              <div
                className={`h-full ${categoryColor(d.category)} transition-all`}
                style={{ width: `${max > 0 ? (d.total / max) * 100 : 0}%` }}
              />
            </div>
            <span className="w-24 shrink-0 text-right text-sm font-medium text-neutral-900">
              {formatBRL(d.total)}
            </span>
          </div>
        ))}
      </div>
    </div>
  )
}
