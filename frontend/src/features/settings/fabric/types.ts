export interface FabricColorDto {
  id: string
  fabricTypeId: string
  name: string
  hexCode: string | null
}

export interface FabricTypeDto {
  id: string
  name: string
  variation: string
  pricePerKg: number
  averageKgPerRoll: number
  averagePiecesPerRoll: number | null
  colors: FabricColorDto[]
}
