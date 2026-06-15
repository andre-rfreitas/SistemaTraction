import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderFormData } from '../schemas/dtfOrderSchema'

export function useUpdateDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: DtfOrderFormData }) =>
      api.put(`/dtf-orders/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
