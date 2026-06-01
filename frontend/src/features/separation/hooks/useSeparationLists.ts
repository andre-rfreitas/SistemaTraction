import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SeparationListSummary } from '../types'

export function useSeparationLists() {
  return useQuery({
    queryKey: ['separation-lists'],
    queryFn: async () => {
      const { data } = await api.get<SeparationListSummary[]>('/separation-lists')
      return data
    },
  })
}
