import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeDto } from '../types'

export function useFabricTypes() {
  return useQuery({
    queryKey: ['fabric-types'],
    queryFn: async () => {
      const { data } = await api.get<FabricTypeDto[]>('/fabric-types')
      return data
    },
  })
}
