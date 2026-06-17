import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { OperationalExpenseDto } from '../types'

const QK = ['opex']

export function useOperationalExpenses() {
  return useQuery({
    queryKey: QK,
    queryFn: async () => {
      const { data } = await api.get<OperationalExpenseDto[]>('/financial/opex')
      return data
    },
  })
}

export function useCreateOpex() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: { name: string; fixedMonthlyValue: number; ratePercent: number }) => {
      const { data } = await api.post<OperationalExpenseDto>('/financial/opex', payload)
      return data
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: QK })
      qc.invalidateQueries({ queryKey: ['financial-summary'] })
    },
  })
}

export function useUpdateOpex() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: { id: string; name: string; fixedMonthlyValue: number; ratePercent: number; isActive: boolean }) => {
      const { data } = await api.put<OperationalExpenseDto>(`/financial/opex/${payload.id}`, payload)
      return data
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: QK })
      qc.invalidateQueries({ queryKey: ['financial-summary'] })
    },
  })
}

export function useDeleteOpex() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/financial/opex/${id}`)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: QK })
      qc.invalidateQueries({ queryKey: ['financial-summary'] })
    },
  })
}
