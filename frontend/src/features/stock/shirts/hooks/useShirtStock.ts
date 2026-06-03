import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtStockGridDto } from '../types'

export function useShirtStock() {
  return useQuery({
    queryKey: ['shirt-stock'],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockGridDto>('/stock/shirts')
      return data
    },
  })
}
