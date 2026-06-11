import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricRollFormData } from '../schemas/fabricRollSchema'

export function useUpdateFabricRoll() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: FabricRollFormData }) => {
      await api.put(`/fabric-rolls/${id}`, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
