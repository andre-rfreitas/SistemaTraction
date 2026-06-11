import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SewerDto, CreateSewerInput, UpdateSewerInput } from '../types'

export function useSewers(includeInactive = false) {
  return useQuery({
    queryKey: ['sewers', includeInactive],
    queryFn: async () => {
      const { data } = await api.get<SewerDto[]>('/sewers', {
        params: { includeInactive },
      })
      return data
    },
  })
}

export function useCreateSewer() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (input: CreateSewerInput) => {
      const { data } = await api.post<SewerDto>('/sewers', input)
      return data
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sewers'] }),
  })
}

export function useUpdateSewer() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, input }: { id: string; input: UpdateSewerInput }) => {
      const { data } = await api.put<SewerDto>(`/sewers/${id}`, input)
      return data
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sewers'] }),
  })
}

export function useDeleteSewer() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/sewers/${id}`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sewers'] }),
  })
}
