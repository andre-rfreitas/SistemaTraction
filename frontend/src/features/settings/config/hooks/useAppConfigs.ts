import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { AppConfigDto } from '../types'

export function useAppConfigs() {
  return useQuery<AppConfigDto[]>({
    queryKey: ['app-configs'],
    queryFn: async () => {
      const { data } = await api.get('/config')
      return data
    },
  })
}
