import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { AdjustShirtStockResult } from '../types'

export interface AdjustPayload {
  fabricColorId: string
  size: string
  adjustmentType: 'Entrada' | 'Saída'
  quantity: number
  reason: string
}

export function useAdjustShirtStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: AdjustPayload) => {
      const { data } = await api.post<AdjustShirtStockResult>('/stock/shirts/adjustment', payload)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
      queryClient.invalidateQueries({ queryKey: ['shirt-stock-movements'] })
    },
  })
}
