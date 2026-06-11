import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { RegisterCuttingDeliveryResult } from '../types'

export interface DeliveryItemInput {
  fabricRollId: string
  deliveredPieces: Record<string, number>
}

export function useRegisterCuttingDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, items }: { orderId: string; items: DeliveryItemInput[] }) => {
      const { data } = await api.post<RegisterCuttingDeliveryResult>(
        `/cutting-orders/${orderId}/delivery`,
        { items }
      )
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
