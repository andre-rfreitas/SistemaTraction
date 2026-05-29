import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricColorFormData } from '../schemas/fabricTypeSchema'

export function useUpdateFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ colorId, data }: { colorId: string; data: FabricColorFormData }) =>
      api.put(`/fabric-types/${fabricTypeId}/colors/${colorId}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
