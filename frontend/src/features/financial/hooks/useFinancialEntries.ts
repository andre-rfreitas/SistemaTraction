import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FinancialEntryDto } from '../types'

interface Params {
  from: string
  to: string
  category?: string | null
}

export function useFinancialEntries({ from, to, category }: Params) {
  return useQuery<FinancialEntryDto[]>({
    queryKey: ['financial-entries', from, to, category ?? null],
    queryFn: async () => {
      const { data } = await api.get('/financial/entries', {
        params: { from, to, category: category || undefined },
      })
      return data
    },
  })
}
