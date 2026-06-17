import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShirtType } from '../types'

export interface ShirtStockItemDto {
  id: string
  fabricColorId: string
  fabricColorName: string
  size: string
  shirtType: string
  quantity: number
}

export function useShirtStockItems(shirtType: ShirtType) {
  return useQuery({
    queryKey: ['shirt-stock-items', shirtType],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockItemDto[]>('/stock/shirts/items', {
        params: { shirtType },
      })
      return data
    },
  })
}
