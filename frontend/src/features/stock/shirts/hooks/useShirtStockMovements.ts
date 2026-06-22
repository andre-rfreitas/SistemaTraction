import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtStockMovementsDto } from '../types'

export function useShirtStockMovements(modelCode: string = 'REG', page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['shirt-stock-movements', modelCode, page, pageSize],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockMovementsDto>(
        `/stock/shirts/movements?modelCode=${modelCode}&page=${page}&pageSize=${pageSize}`
      )
      return data
    },
  })
}
