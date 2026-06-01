import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { StockCheckResult } from '../types'

export function useStockCheck(listId: string | null) {
  return useQuery({
    queryKey: ['separation-stock-check', listId],
    queryFn: async () => {
      const { data } = await api.get<StockCheckResult>(`/separation-lists/${listId}/stock-check`)
      return data
    },
    enabled: !!listId,
  })
}
