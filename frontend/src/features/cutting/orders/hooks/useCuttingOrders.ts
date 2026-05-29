import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CuttingOrderDto } from '../types'

export function useCuttingOrders(status?: string) {
  return useQuery({
    queryKey: ['cutting-orders', status],
    queryFn: async () => {
      const { data } = await api.get<CuttingOrderDto[]>('/cutting-orders', {
        params: status ? { status } : undefined,
      })
      return data
    },
  })
}
