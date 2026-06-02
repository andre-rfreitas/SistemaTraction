import type { FinancialSummaryDto } from '../types'
import { formatBRL } from '../format'
import { StatCard } from '@/components/ui/stat-card'

interface Props {
  summary: FinancialSummaryDto
}

export function SummaryCards({ summary }: Props) {
  const costs: { label: string; value: number }[] = [
    { label: 'Tecido', value: summary.totalFabric },
    { label: 'Corte', value: summary.totalCutting },
    { label: 'Costura', value: summary.totalSewing },
    { label: 'Defeitos', value: summary.totalDefects },
    { label: 'DTF', value: summary.totalDtf },
    { label: 'Receitas', value: summary.totalIncome },
  ]

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-6">
        {costs.map((c) => (
          <StatCard key={c.label} label={c.label} value={formatBRL(c.value)} />
        ))}
      </div>

      <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
        <StatCard
          label="Saldo do período"
          value={formatBRL(summary.balance)}
          tone={summary.balance >= 0 ? 'success' : 'danger'}
          hint={`Receitas ${formatBRL(summary.totalIncome)} − Despesas ${formatBRL(summary.totalExpense)}`}
        />
        <StatCard
          label="Custo médio / camiseta"
          value={summary.averageCostPerShirt != null ? formatBRL(summary.averageCostPerShirt) : '—'}
          hint="(Tecido + Corte + Costura) ÷ peças boas"
        />
        <StatCard
          label="Peças boas produzidas"
          value={summary.goodPiecesProduced}
          hint="no período selecionado"
        />
      </div>
    </div>
  )
}
