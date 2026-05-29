export interface FabricRollDto {
  id: string
  fabricTypeId: string
  fabricTypeName: string
  fabricTypeVariation: string
  fabricTypePricePerKg: number
  fabricColorId: string
  fabricColorName: string
  fabricColorHexCode: string | null
  weightKg: number
  priceTotal: number
  pricePerKgActual: number
  receivedAt: string
  status: 'Available' | 'InCutting' | 'Consumed'
  createdAt: string
}

export interface RegisterFabricRollResult {
  fabricRollId: string
  financialEntryAmount: number
  fabricTypePricePerKg: number
}
