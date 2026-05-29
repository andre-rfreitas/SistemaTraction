import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfModelFormData } from '../schemas/dtfModelSchema'

export function useUpdateDtfModel() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: DtfModelFormData }) =>
      api.put(`/dtf-models/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-models'] }),
  })
}
