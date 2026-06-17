import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export interface GenericProductCategory {
  id: string
  name: string
}

export interface GenericProduct {
  id: string
  categoryId: string
  name: string
  quantity: number
}

export interface GenericProductMovement {
  id: string
  date: string
  productName: string
  delta: number
  reason: string
  origin: string
}

export interface GenericProductMovementsResponse {
  items: GenericProductMovement[]
  totalCount: number
  page: number
  pageSize: number
}

export function useGenericProductCategories() {
  return useQuery({
    queryKey: ['generic-categories'],
    queryFn: async () => {
      const { data } = await api.get<GenericProductCategory[]>('/stock/generic-products/categories')
      return data
    },
  })
}

export function useGenericProducts(categoryId: string) {
  return useQuery({
    queryKey: ['generic-products', categoryId],
    queryFn: async () => {
      const { data } = await api.get<GenericProduct[]>(`/stock/generic-products/categories/${categoryId}/products`)
      return data
    },
    enabled: !!categoryId,
  })
}

export function useGenericProductMovements(productId: string, page = 1) {
  return useQuery({
    queryKey: ['generic-product-movements', productId, page],
    queryFn: async () => {
      const { data } = await api.get<GenericProductMovementsResponse>(`/stock/generic-products/products/${productId}/movements`, {
        params: { page, pageSize: 20 },
      })
      return data
    },
    enabled: !!productId,
  })
}

export function useCreateGenericCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const { data } = await api.post('/stock/generic-products/categories', { name })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['generic-categories'] })
    },
  })
}

export function useDeleteGenericCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/stock/generic-products/categories/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['generic-categories'] })
    },
  })
}

export function useCreateGenericProduct() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: { categoryId: string; name: string; initialQuantity: number; unitCost?: number; reason?: string }) => {
      const { data } = await api.post('/stock/generic-products/products', payload)
      return data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['generic-products', variables.categoryId] })
    },
  })
}

export function useAdjustGenericProductStock() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: { productId: string; categoryId: string; adjustmentType: 'Entrada' | 'Saída'; quantity: number; reason: string; unitCost?: number }) => {
      const { data } = await api.post(`/stock/generic-products/products/${payload.productId}/adjustment`, {
        adjustmentType: payload.adjustmentType,
        quantity: payload.quantity,
        reason: payload.reason,
        unitCost: payload.unitCost,
      })
      return data
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['generic-products', variables.categoryId] })
      queryClient.invalidateQueries({ queryKey: ['generic-product-movements', variables.productId] })
    },
  })
}

export function useDeleteGenericProduct() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: { productId: string; categoryId: string }) => {
      await api.delete(`/stock/generic-products/products/${payload.productId}`)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['generic-products', variables.categoryId] })
    },
  })
}
