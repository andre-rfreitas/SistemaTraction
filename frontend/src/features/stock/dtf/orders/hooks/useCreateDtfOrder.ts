import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderFormData } from '../schemas/dtfOrderSchema'

export function useCreateDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: DtfOrderFormData) => api.post('/dtf-orders', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
