import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteFabricRoll() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/fabric-rolls/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
