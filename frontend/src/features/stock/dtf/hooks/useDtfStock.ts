import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfStockItemDto } from '../types'

export function useDtfStock() {
  return useQuery<DtfStockItemDto[]>({
    queryKey: ['dtf-stock'],
    queryFn: async () => {
      const { data } = await api.get('/dtf-stock')
      return data
    },
  })
}
