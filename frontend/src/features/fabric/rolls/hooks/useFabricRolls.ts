import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricRollDto } from '../types'

export function useFabricRolls(status?: string) {
  return useQuery({
    queryKey: ['fabric-rolls', status],
    queryFn: async () => {
      const { data } = await api.get<FabricRollDto[]>('/fabric-rolls', {
        params: status ? { status } : undefined,
      })
      return data
    },
  })
}
