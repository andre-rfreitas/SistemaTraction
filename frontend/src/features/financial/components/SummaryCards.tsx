import type { FinancialSummaryDto } from '../types'
import { formatBRL } from '../format'

interface Props {
  summary: FinancialSummaryDto
}

interface Card {
  label: string
  value: string
  accent: string
}

export function SummaryCards({ summary }: Props) {
  const cards: Card[] = [
    { label: 'Tecido', value: formatBRL(summary.totalFabric), accent: 'text-indigo-600' },
    { label: 'Corte', value: formatBRL(summary.totalCutting), accent: 'text-amber-600' },
    { label: 'Costura', value: formatBRL(summary.totalSewing), accent: 'text-emerald-600' },
    { label: 'Defeitos', value: formatBRL(summary.totalDefects), accent: 'text-red-600' },
    { label: 'DTF', value: formatBRL(summary.totalDtf), accent: 'text-fuchsia-600' },
    { label: 'Receitas', value: formatBRL(summary.totalIncome), accent: 'text-green-700' },
  ]

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
        {cards.map((c) => (
          <div key={c.label} className="rounded-lg border border-neutral-200 bg-white p-4">
            <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">{c.label}</p>
            <p className={`mt-1 text-lg font-bold ${c.accent}`}>{c.value}</p>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        <div className="rounded-lg border border-neutral-900 bg-neutral-900 p-4 text-white">
          <p className="text-xs font-semibold text-neutral-300 uppercase tracking-wide">Saldo do período</p>
          <p className={`mt-1 text-2xl font-bold ${summary.balance >= 0 ? 'text-green-400' : 'text-red-400'}`}>
            {formatBRL(summary.balance)}
          </p>
          <p className="mt-1 text-xs text-neutral-400">
            Receitas {formatBRL(summary.totalIncome)} − Despesas {formatBRL(summary.totalExpense)}
          </p>
        </div>

        <div className="rounded-lg border border-neutral-200 bg-white p-4">
          <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Custo médio / camiseta</p>
          <p className="mt-1 text-2xl font-bold text-neutral-900">
            {summary.averageCostPerShirt != null ? formatBRL(summary.averageCostPerShirt) : '—'}
          </p>
          <p className="mt-1 text-xs text-neutral-400">(Tecido + Corte + Costura) ÷ peças boas</p>
        </div>

        <div className="rounded-lg border border-neutral-200 bg-white p-4">
          <p className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Peças boas produzidas</p>
          <p className="mt-1 text-2xl font-bold text-neutral-900">{summary.goodPiecesProduced}</p>
          <p className="mt-1 text-xs text-neutral-400">no período selecionado</p>
        </div>
      </div>
    </div>
  )
}
