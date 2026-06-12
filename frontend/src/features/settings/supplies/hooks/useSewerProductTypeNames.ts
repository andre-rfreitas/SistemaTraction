import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useSewerProductTypeNames() {
  return useQuery({
    queryKey: ['sewer-product-type-names'],
    queryFn: async () => {
      const { data } = await api.get<string[]>('/sewers/product-type-names')
      return data
    },
  })
}
