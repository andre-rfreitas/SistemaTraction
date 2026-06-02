import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FinancialEntryFormData } from '../schemas/financialEntrySchema'

export function useCreateFinancialEntry() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: FinancialEntryFormData) =>
      api.post('/financial/entries', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-summary'] })
      queryClient.invalidateQueries({ queryKey: ['financial-entries'] })
    },
  })
}
