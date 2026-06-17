import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtStockMovementsDto, ShirtType } from '../types'

export function useShirtStockMovements(shirtType: ShirtType = 'Regular', page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['shirt-stock-movements', shirtType, page, pageSize],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockMovementsDto>(
        `/stock/shirts/movements?shirtType=${shirtType}&page=${page}&pageSize=${pageSize}`
      )
      return data
    },
  })
}
