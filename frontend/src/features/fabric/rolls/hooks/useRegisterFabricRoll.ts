import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricRollFormData } from '../schemas/fabricRollSchema'
import type { RegisterFabricRollResult } from '../types'

export function useRegisterFabricRoll() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: FabricRollFormData) => {
      const { data: result } = await api.post<RegisterFabricRollResult>('/fabric-rolls', data)
      return result
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
