import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyTypeFormData } from '../schemas/supplyTypeSchema'

export function useUpdateSupplyType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SupplyTypeFormData }) =>
      api.put(`/supply-types/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-types'] })
      queryClient.invalidateQueries({ queryKey: ['supply-stock'] })
      queryClient.invalidateQueries({ queryKey: ['supply-order-configs'] })
    },
  })
}
