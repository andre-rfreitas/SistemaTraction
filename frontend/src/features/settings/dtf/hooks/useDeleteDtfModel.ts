import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteDtfModel() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/dtf-models/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-models'] }),
  })
}
