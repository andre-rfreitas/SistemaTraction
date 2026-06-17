import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteShirtStockItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (stockItemId: string) => {
      const { data } = await api.delete(`/stock/shirts/items/${stockItemId}`)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
      queryClient.invalidateQueries({ queryKey: ['shirt-stock-movements'] })
    },
  })
}
