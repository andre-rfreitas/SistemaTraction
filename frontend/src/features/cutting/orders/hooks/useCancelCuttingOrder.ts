import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useCancelCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      await api.delete(`/cutting-orders/${orderId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
