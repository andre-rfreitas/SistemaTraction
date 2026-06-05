import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyStockMovementDto } from '../types'

export function useSupplyStockMovements(supplyStockItemId: string | null) {
  return useQuery<SupplyStockMovementDto[]>({
    queryKey: ['supply-stock-movements', supplyStockItemId],
    queryFn: async () => {
      const { data } = await api.get(`/supply-stock/${supplyStockItemId}/movements`)
      return data
    },
    enabled: !!supplyStockItemId,
  })
}
