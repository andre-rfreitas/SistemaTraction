import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { RegisterSupplyMovementResult } from '../types'

interface Payload {
  supplyStockItemId: string
  type: 'Entrada' | 'Saida' | 'Ajuste'
  quantity: number
  reason?: string | null
}

export function useRegisterSupplyMovement() {
  const queryClient = useQueryClient()
  return useMutation<RegisterSupplyMovementResult, Error, Payload>({
    mutationFn: async ({ supplyStockItemId, ...body }) => {
      const { data } = await api.post(`/supply-stock/${supplyStockItemId}/movements`, body)
      return data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['supply-stock'] })
      queryClient.invalidateQueries({ queryKey: ['supply-stock-movements', variables.supplyStockItemId] })
    },
  })
}
