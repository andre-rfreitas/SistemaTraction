import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { RegisterCuttingDeliveryResult } from '../types'

export function useRegisterCuttingDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, deliveredPieces }: { orderId: string; deliveredPieces: Record<string, number> }) => {
      const { data } = await api.post<RegisterCuttingDeliveryResult>(
        `/cutting-orders/${orderId}/delivery`,
        { deliveredPieces }
      )
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
