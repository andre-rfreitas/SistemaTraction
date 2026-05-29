import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfModelFormData } from '../schemas/dtfModelSchema'

export function useCreateDtfModel() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: DtfModelFormData) => api.post('/dtf-models', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-models'] }),
  })
}
