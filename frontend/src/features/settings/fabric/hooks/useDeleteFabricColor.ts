import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (colorId: string) =>
      api.delete(`/fabric-types/${fabricTypeId}/colors/${colorId}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
