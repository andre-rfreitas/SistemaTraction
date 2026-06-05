import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyOrderConfigDto } from '@/features/stock/supplies/types'

export function useSupplyOrderConfigs() {
  return useQuery<SupplyOrderConfigDto[]>({
    queryKey: ['supply-order-configs'],
    queryFn: async () => {
      const { data } = await api.get('/supply-order-configs')
      return data
    },
  })
}
