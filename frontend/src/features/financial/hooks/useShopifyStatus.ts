import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShopifyStatusDto } from '../types'

export function useShopifyStatus() {
  return useQuery<ShopifyStatusDto>({
    queryKey: ['shopify-status'],
    queryFn: async () => {
      const { data } = await api.get('/financial/shopify/status')
      return data
    },
  })
}
