import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfMovementType } from '../types'

interface RegisterMovementPayload {
  dtfModelId: string
  type: DtfMovementType
  quantity: number
  reason: string | null
}

export function useRegisterDtfMovement() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: RegisterMovementPayload) =>
      api.post('/dtf-stock/movements', payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['dtf-stock'] })
      queryClient.invalidateQueries({ queryKey: ['dtf-stock', variables.dtfModelId] })
    },
  })
}
