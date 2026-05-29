import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/fabric-types/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
