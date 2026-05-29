import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeFormData } from '../schemas/fabricTypeSchema'

export function useCreateFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: FabricTypeFormData) => api.post('/fabric-types', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
