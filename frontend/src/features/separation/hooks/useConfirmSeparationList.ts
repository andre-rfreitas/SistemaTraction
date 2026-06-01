import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SeparationConfirmResult } from '../types'

export function useConfirmSeparationList() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (listId: string) => {
      const { data } = await api.post<SeparationConfirmResult>(`/separation-lists/${listId}/confirm`)
      return data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['separation-lists'] }),
  })
}
