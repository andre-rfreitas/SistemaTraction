import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useCancelDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/dtf-orders/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
