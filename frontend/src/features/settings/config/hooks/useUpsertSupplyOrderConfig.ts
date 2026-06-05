import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useUpsertSupplyOrderConfig() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: { supplyTypeId: string; quantityPerOrder: number }) =>
      api.post('/supply-order-configs', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-order-configs'] })
    },
  })
}
