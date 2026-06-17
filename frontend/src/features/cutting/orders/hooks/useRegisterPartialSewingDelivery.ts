import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { RegisterSewingDeliveryResult } from '../types'
import type { SewingDeliveryItemInput } from './useRegisterSewingDelivery'

export function useRegisterPartialSewingDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      orderId,
      items,
    }: {
      orderId: string
      items: SewingDeliveryItemInput[]
    }) => {
      const { data } = await api.post<RegisterSewingDeliveryResult>(
        `/cutting-orders/${orderId}/partial-sewing-delivery`,
        { items },
      )
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['stock-items'] })
    },
  })
}
