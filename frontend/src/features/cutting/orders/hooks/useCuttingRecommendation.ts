import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CuttingRecommendationDto } from '../types'

export function useCuttingRecommendation(fabricRollId: string | null, daysBack?: number) {
  return useQuery({
    queryKey: ['cutting-recommendation', fabricRollId, daysBack],
    queryFn: async () => {
      const params = new URLSearchParams({ fabricRollId: fabricRollId! })
      if (daysBack) params.set('daysBack', String(daysBack))
      const { data } = await api.get<CuttingRecommendationDto>(
        `/cutting-orders/recommendation?${params}`
      )
      return data
    },
    enabled: !!fabricRollId,
    staleTime: 30_000,
  })
}
