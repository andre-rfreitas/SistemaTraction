import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export interface UpdateSupplyMovementInput {
  supplierName?: string | null
  supplierPhone?: string | null
  occurredAt?: string | null
  unitPrice?: number | null
  totalCost?: number | null
}

export function useUpdateSupplyMovement(supplyStockItemId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ movementId, data }: { movementId: string; data: UpdateSupplyMovementInput }) => {
      await api.put(`/supply-stock/movements/${movementId}`, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-stock-movements', supplyStockItemId] })
    },
  })
}
