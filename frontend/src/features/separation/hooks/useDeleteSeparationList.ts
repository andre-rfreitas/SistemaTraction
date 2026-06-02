import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteSeparationList() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/separation-lists/${id}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['separation-lists'] }),
  })
}
