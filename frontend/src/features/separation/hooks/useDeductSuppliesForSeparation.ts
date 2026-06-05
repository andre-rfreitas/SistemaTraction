import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

interface DeductItem {
  supplyStockItemId: string
  quantity: number
}

export function useDeductSuppliesForSeparation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (items: DeductItem[]) =>
      api.post('/supply-deduction/deduct', { items }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supply-stock'] })
    },
  })
}
