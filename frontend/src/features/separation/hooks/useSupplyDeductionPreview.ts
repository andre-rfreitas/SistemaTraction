import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SupplyDeductionPreviewItem } from '@/features/stock/supplies/types'

export function useSupplyDeductionPreview(orderCount: number | null) {
  return useQuery<SupplyDeductionPreviewItem[]>({
    queryKey: ['supply-deduction-preview', orderCount],
    queryFn: async () => {
      const { data } = await api.get('/supply-deduction/preview', { params: { orderCount } })
      return data
    },
    enabled: orderCount !== null && orderCount > 0,
  })
}
