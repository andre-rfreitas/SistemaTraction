import { useMutation } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SeparationListDetail, UpdateItemPayload } from '../types'

export function useUpdateSeparationItems() {
  return useMutation({
    mutationFn: async ({ listId, items }: { listId: string; items: UpdateItemPayload[] }) => {
      const { data } = await api.put<SeparationListDetail>(`/separation-lists/${listId}/items`, { items })
      return data
    },
  })
}
