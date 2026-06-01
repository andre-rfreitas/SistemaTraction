import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SkuCodeDto, SkuCodeCategory } from '../types'

export function useSkuCodes() {
  return useQuery({
    queryKey: ['sku-codes'],
    queryFn: async () => {
      const { data } = await api.get<SkuCodeDto[]>('/separation-lists/sku-codes')
      return data
    },
  })
}

export function useUpsertSkuCode() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      id?: string
      code: string
      value: string
      category: SkuCodeCategory
    }) => {
      const { data } = await api.post<SkuCodeDto>('/separation-lists/sku-codes', payload)
      return data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sku-codes'] }),
  })
}

export function useDeleteSkuCode() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/separation-lists/sku-codes/${id}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sku-codes'] }),
  })
}
