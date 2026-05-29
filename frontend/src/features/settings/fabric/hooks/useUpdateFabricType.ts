import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeFormData } from '../schemas/fabricTypeSchema'

export function useUpdateFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: FabricTypeFormData }) =>
      api.put(`/fabric-types/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
