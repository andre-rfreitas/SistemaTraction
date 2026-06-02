import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SeparationListSummary } from '../types'

export function useRenameSeparationList() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, fileName }: { id: string; fileName: string }) => {
      const { data } = await api.patch<SeparationListSummary>(
        `/separation-lists/${id}/rename`,
        { fileName },
      )
      return data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['separation-lists'] }),
  })
}
