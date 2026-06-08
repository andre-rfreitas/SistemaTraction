import { useMemo, useState } from 'react'
import { useFinancialSummary } from './hooks/useFinancialSummary'
import { useFinancialEntries } from './hooks/useFinancialEntries'
import { useCreateFinancialEntry } from './hooks/useCreateFinancialEntry'
import { useReverseFinancialEntry } from './hooks/useReverseFinancialEntry'
import { SummaryCards } from './components/SummaryCards'
import { CategoryChart } from './components/CategoryChart'
import { EntriesTable } from './components/EntriesTable'
import { ManualEntryForm } from './components/ManualEntryForm'
import { ShopifySyncCard } from './components/ShopifySyncCard'
import type { PeriodPreset } from './types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

function toIso(date: Date): string {
  return date.toISOString()
}

function startOfDay(d: Date): Date {
  const c = new Date(d)
  c.setHours(0, 0, 0, 0)
  return c
}

function endOfDay(d: Date): Date {
  const c = new Date(d)
  c.setHours(23, 59, 59, 999)
  return c
}

function presetRange(preset: PeriodPreset): { from: string; to: string } {
  const now = new Date()
  if (preset === 'week') {
    const start = startOfDay(now)
    start.setDate(start.getDate() - 6)
    return { from: toIso(start), to: toIso(endOfDay(now)) }
  }
  // month
  const start = startOfDay(new Date(now.getFullYear(), now.getMonth(), 1))
  return { from: toIso(start), to: toIso(endOfDay(now)) }
}

export function FinancialPage() {
  const [preset, setPreset] = useState<PeriodPreset>('month')
  const [customFrom, setCustomFrom] = useState('')
  const [customTo, setCustomTo] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('')
  const [search, setSearch] = useState('')
  const [showForm, setShowForm] = useState(false)

  const range = useMemo(() => {
    if (preset === 'custom') {
      if (!customFrom || !customTo) return presetRange('month')
      return {
        from: toIso(startOfDay(new Date(customFrom))),
        to: toIso(endOfDay(new Date(customTo))),
      }
    }
    return presetRange(preset)
  }, [preset, customFrom, customTo])

  const summary = useFinancialSummary(range.from, range.to)
  const entries = useFinancialEntries({ from: range.from, to: range.to })
  const create = useCreateFinancialEntry()
  const reverse = useReverseFinancialEntry()

  const categories = useMemo(() => {
    const set = new Set((entries.data ?? []).map((e) => e.category))
    return [...set].sort()
  }, [entries.data])

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase()
    return (entries.data ?? []).filter((e) => {
      if (categoryFilter && e.category !== categoryFilter) return false
      if (term && !`${e.description} ${e.category}`.toLowerCase().includes(term)) return false
      return true
    })
  }, [entries.data, categoryFilter, search])

  return (
    <div className="space-y-6">
      <PageHeader
        title="Financeiro"
        description="Controle de custos, receitas e saldo do período."
        actions={
          <Button onClick={() => setShowForm(true)}>+ Lançamento manual</Button>
        }
      />

      <div className="flex items-end gap-2 flex-wrap">
        <div className="flex gap-1 rounded-md border border-border p-1">
          {(['week', 'month', 'custom'] as PeriodPreset[]).map((p) => (
            <button
              key={p}
              onClick={() => setPreset(p)}
              className={`px-3 py-1 text-sm rounded transition-colors ${
                preset === p
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:text-foreground'
              }`}
            >
              {p === 'week' ? 'Semana' : p === 'month' ? 'Mês' : 'Personalizado'}
            </button>
          ))}
        </div>
        {preset === 'custom' && (
          <>
            <div className="space-y-1">
              <Label htmlFor="from" className="text-xs">De</Label>
              <Input id="from" type="date" value={customFrom} onChange={(e) => setCustomFrom(e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="to" className="text-xs">Até</Label>
              <Input id="to" type="date" value={customTo} onChange={(e) => setCustomTo(e.target.value)} />
            </div>
          </>
        )}
      </div>

      {summary.isLoading && (
        <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
      )}
      {summary.isError && (
        <p className="text-sm text-danger">{(summary.error as Error).message}</p>
      )}
      {summary.data && (
        <>
          <SummaryCards summary={summary.data} />
          <CategoryChart data={summary.data.expenseByCategory} />
        </>
      )}

      {entries.data && (
        <ShopifySyncCard
          entries={entries.data}
          periodFrom={range.from}
          periodTo={range.to}
        />
      )}

      <div className="space-y-3">
        <div className="flex items-center gap-2 flex-wrap">
          <h3 className="text-sm font-semibold text-foreground">Lançamentos</h3>
          <div className="flex-1" />
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
          >
            <option value="">Todas as categorias</option>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
          <Input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Buscar descrição..."
            className="w-48"
          />
        </div>

        {entries.isLoading ? (
          <Skeleton className="h-64 w-full" />
        ) : (
          <EntriesTable
            entries={filtered}
            onReverse={(id) => reverse.mutate(id)}
            isReversing={reverse.isPending}
          />
        )}
        {reverse.isError && (
          <p className="text-sm text-danger">{(reverse.error as Error).message}</p>
        )}
      </div>

      <Dialog open={showForm} onOpenChange={setShowForm}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Novo lançamento manual</DialogTitle>
          </DialogHeader>
          <ManualEntryForm
            isLoading={create.isPending}
            onSubmit={(data) =>
              create.mutate(data, { onSuccess: () => setShowForm(false) })
            }
          />
          {create.isError && (
            <p className="text-sm text-danger mt-2">{(create.error as Error).message}</p>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
