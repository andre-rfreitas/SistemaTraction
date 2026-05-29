import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SendCuttingOrderResult } from '../types'

export function useSendCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      const { data } = await api.post<SendCuttingOrderResult>(`/cutting-orders/${orderId}/send`)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
