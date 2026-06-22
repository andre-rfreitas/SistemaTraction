import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

export interface ShirtStockItemDto {
  id: string
  fabricColorId: string
  fabricColorName: string
  size: string
  modelCode: string
  quantity: number
}

export function useShirtStockItems(modelCode: string) {
  return useQuery({
    queryKey: ['shirt-stock-items', modelCode],
    queryFn: async () => {
      const { data } = await api.get<ShirtStockItemDto[]>('/stock/shirts/items', {
        params: { modelCode },
      })
      return data
    },
  })
}
