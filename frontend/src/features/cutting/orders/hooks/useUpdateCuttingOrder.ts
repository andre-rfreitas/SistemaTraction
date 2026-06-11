import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { UpdateCuttingOrderInput } from '../types'

export function useUpdateCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, items, notes }: UpdateCuttingOrderInput) => {
      await api.put(`/cutting-orders/${orderId}`, { items, notes })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
    },
  })
}
