import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { ShopifySyncResultDto } from '../types'

export function useShopifySync() {
  const queryClient = useQueryClient()

  return useMutation<ShopifySyncResultDto, Error, { from: string; to: string }>({
    mutationFn: async ({ from, to }) => {
      const { data } = await api.post('/financial/shopify/sync', { from, to })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-entries'] })
      queryClient.invalidateQueries({ queryKey: ['financial-summary'] })
      queryClient.invalidateQueries({ queryKey: ['shopify-status'] })
    },
  })
}
