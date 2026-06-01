import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { RegisterSewingDeliveryResult } from '../types'

export function useRegisterSewingDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      orderId,
      goodPieces,
      defectivePieces,
    }: {
      orderId: string
      goodPieces: Record<string, number>
      defectivePieces: Record<string, number>
    }) => {
      const { data } = await api.post<RegisterSewingDeliveryResult>(
        `/cutting-orders/${orderId}/sewing-delivery`,
        { goodPieces, defectivePieces },
      )
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['stock-items'] })
    },
  })
}
