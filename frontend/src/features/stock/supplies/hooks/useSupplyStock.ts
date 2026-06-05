import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyStockItemDto } from '../types'

export function useSupplyStock() {
  return useQuery<SupplyStockItemDto[]>({
    queryKey: ['supply-stock'],
    queryFn: async () => {
      const { data } = await api.get('/supply-stock')
      return data
    },
  })
}
