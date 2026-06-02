import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FinancialSummaryDto } from '../types'

export function useFinancialSummary(from: string, to: string) {
  return useQuery<FinancialSummaryDto>({
    queryKey: ['financial-summary', from, to],
    queryFn: async () => {
      const { data } = await api.get('/financial/summary', { params: { from, to } })
      return data
    },
  })
}
