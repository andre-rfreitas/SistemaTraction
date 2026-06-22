import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtStockGridDto } from '../types'

export function useShirtStock(modelCode: string = 'REG') {
  return useQuery({
    queryKey: ['shirt-stock', modelCode],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockGridDto>(`/stock/shirts?modelCode=${modelCode}`)
      return data
    },
  })
}
