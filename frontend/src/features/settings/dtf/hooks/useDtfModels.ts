import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfModelDto } from '../types'

export function useDtfModels() {
  return useQuery({
    queryKey: ['dtf-models'],
    queryFn: async () => {
      const { data } = await api.get<DtfModelDto[]>('/dtf-models')
      return data
    },
  })
}
