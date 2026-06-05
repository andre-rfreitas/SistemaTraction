import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyTypeFormData } from '../schemas/supplyTypeSchema'

export function useCreateSupplyType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: SupplyTypeFormData) => api.post('/supply-types', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-types'] })
      queryClient.invalidateQueries({ queryKey: ['supply-stock'] })
      queryClient.invalidateQueries({ queryKey: ['supply-order-configs'] })
    },
  })
}
