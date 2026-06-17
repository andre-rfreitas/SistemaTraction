export type FinancialEntryType = 'Income' | 'Expense'

export interface FinancialEntryDto {
  id: string
  type: FinancialEntryType
  category: string
  amount: number
  description: string
  referenceId: string | null
  referenceType: string | null
  entryDate: string
  isReversal: boolean
  isReversed: boolean
  createdAt: string
}

export interface CategoryTotalDto {
  category: string
  total: number
}

export interface OpexPeriodItemDto {
  id: string
  name: string
  fixedMonthlyValue: number
  ratePercent: number
  proratedFixed: number
  rateAmount: number
  periodTotal: number
}

export interface OperationalExpenseDto {
  id: string
  name: string
  fixedMonthlyValue: number
  ratePercent: number
  isActive: boolean
}

export interface FinancialSummaryDto {
  from: string
  to: string
  totalFabric: number
  totalCutting: number
  totalSewing: number
  totalDefects: number
  totalDtf: number
  totalIncome: number
  totalExpense: number
  balance: number
  goodPiecesProduced: number
  averageCostPerShirt: number | null
  expenseByCategory: CategoryTotalDto[]
  totalOpex: number
  opexItems: OpexPeriodItemDto[]
}

export interface ShopifySyncResultDto {
  totalImported: number
  totalSkipped: number
  totalAmount: number
  errors: string[]
}

export interface ShopifyStatusDto {
  configured: boolean
  lastSync: string | null
  lastSyncImported: number
  lastSyncAmount: number
}

export type PeriodPreset = 'week' | 'month' | 'custom'

export const CATEGORY_COLORS: Record<string, string> = {
  Tecido: 'bg-indigo-500',
  Corte: 'bg-amber-500',
  Costura: 'bg-emerald-500',
  Defeitos: 'bg-red-500',
  DTF: 'bg-fuchsia-500',
}

export function categoryColor(category: string): string {
  return CATEGORY_COLORS[category] ?? 'bg-neutral-400'
}
