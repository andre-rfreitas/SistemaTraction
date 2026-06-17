import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtStockGridDto, ShirtType } from '../types'

export function useShirtStock(shirtType: ShirtType = 'Regular') {
  return useQuery({
    queryKey: ['shirt-stock', shirtType],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockGridDto>(`/stock/shirts?shirtType=${shirtType}`)
      return data
    },
  })
}
