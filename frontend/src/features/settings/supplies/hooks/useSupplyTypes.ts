import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyTypeDto } from '../types'

export function useSupplyTypes() {
  return useQuery<SupplyTypeDto[]>({
    queryKey: ['supply-types'],
    queryFn: async () => {
      const { data } = await api.get('/supply-types')
      return data
    },
  })
}
