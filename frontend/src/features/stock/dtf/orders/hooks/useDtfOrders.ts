import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderDto, DtfOrderStatus } from '../types'

export function useDtfOrders(status?: DtfOrderStatus) {
  return useQuery({
    queryKey: ['dtf-orders', status],
    queryFn: async () => {
      const params = status ? { status } : undefined
      const { data } = await api.get<DtfOrderDto[]>('/dtf-orders', { params })
      return data
    },
  })
}
