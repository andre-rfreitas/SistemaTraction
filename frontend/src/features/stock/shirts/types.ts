export interface ShirtStockGridDto {
  sizes: string[]
  rows: ShirtStockRowDto[]
  totalsPerSize: Record<string, number>
  grandTotal: number
  alertThreshold: number
}

export interface ShirtStockRowDto {
  colorId: string
  colorName: string
  quantities: Record<string, number>
  total: number
}

export interface ShirtStockMovementsDto {
  items: ShirtStockMovementDto[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ShirtStockMovementDto {
  id: string
  date: string
  fabricColorName: string
  size: string
  delta: number
  reason: string
  origin: string
}

export interface AdjustShirtStockResult {
  movementId: string
  fabricColorName: string
  size: string
  delta: number
  newQuantity: number
}
