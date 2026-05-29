import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfStockItemDetailDto } from '../types'

export function useDtfStockDetail(dtfModelId: string | null) {
  return useQuery<DtfStockItemDetailDto>({
    queryKey: ['dtf-stock', dtfModelId],
    queryFn: async () => {
      const { data } = await api.get(`/dtf-stock/${dtfModelId}`)
      return data
    },
    enabled: !!dtfModelId,
    retry: false,
  })
}
