import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CreateCuttingOrderResult } from '../types'

export interface CreateCuttingOrderPayload {
  fabricRollId: string
  requestedPieces: Record<string, number>
  notes?: string
  recommendedPieces?: Record<string, number> | null
  recommendationDays?: number | null
  recommendationBasedOnOrders?: number | null
}

export function useCreateCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateCuttingOrderPayload) => {
      const { data: result } = await api.post<CreateCuttingOrderResult>('/cutting-orders', data)
      return result
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
      queryClient.invalidateQueries({ queryKey: ['cutting-recommendation-history'] })
    },
  })
}
