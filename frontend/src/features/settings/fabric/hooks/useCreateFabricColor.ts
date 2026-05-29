import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricColorFormData } from '../schemas/fabricTypeSchema'

export function useCreateFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: FabricColorFormData) =>
      api.post(`/fabric-types/${fabricTypeId}/colors`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
