import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CuttingRecommendationHistoryItemDto } from '../types'

export function useCuttingRecommendationHistory(take = 10) {
  return useQuery({
    queryKey: ['cutting-recommendation-history', take],
    queryFn: async () => {
      const { data } = await api.get<CuttingRecommendationHistoryItemDto[]>(
        `/cutting-orders/recommendation-history?take=${take}`
      )
      return data
    },
    staleTime: 60_000,
  })
}
