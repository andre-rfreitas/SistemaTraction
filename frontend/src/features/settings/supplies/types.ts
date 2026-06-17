export type YieldBasis = 'None' | 'PerOrder' | 'PerProduct' | 'PerAllProducts'

export interface SupplyTypeDto {
  id: string
  name: string
  unit: string
  pricePerUnit: number | null
  yieldBasis: YieldBasis
  yieldQuantity: number | null
  yieldProductName: string | null
}
