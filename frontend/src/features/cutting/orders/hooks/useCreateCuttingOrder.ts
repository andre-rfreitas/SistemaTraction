import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CuttingOrderFormData } from '../schemas/cuttingOrderSchema'
import type { CreateCuttingOrderResult } from '../types'

export function useCreateCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CuttingOrderFormData) => {
      const { data: result } = await api.post<CreateCuttingOrderResult>('/cutting-orders', data)
      return result
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
    },
  })
}
