import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteSupplyType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/supply-types/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-types'] })
      queryClient.invalidateQueries({ queryKey: ['supply-stock'] })
      queryClient.invalidateQueries({ queryKey: ['supply-order-configs'] })
    },
  })
}
