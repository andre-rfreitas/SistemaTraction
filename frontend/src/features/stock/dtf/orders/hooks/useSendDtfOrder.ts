import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useSendDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.post(`/dtf-orders/${id}/send`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
