export interface SewerProductTypeDto {
  id: string
  name: string
  priceDefault: number
  priceG1: number
}

export interface SewerDto {
  id: string
  name: string
  phone: string | null
  isActive: boolean
  productTypes: SewerProductTypeDto[]
}

export interface ProductTypeInput {
  name: string
  priceDefault: number
  priceG1: number
}

export interface CreateSewerInput {
  name: string
  phone?: string
  productTypes: ProductTypeInput[]
}

export interface UpdateSewerInput {
  name: string
  phone?: string
  productTypes: ProductTypeInput[]
}
