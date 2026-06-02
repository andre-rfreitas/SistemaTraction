export type DtfMovementType = 1 | 2 | 3 // Entrada | Saida | Ajuste

export interface DtfStockItemDto {
  id: string
  dtfModelId: string
  modelName: string
  sheetLabel: string
  currentQuantity: number
}

export interface DtfStockMovementDto {
  id: string
  type: DtfMovementType
  delta: number
  reason: string | null
  createdAt: string
}

export interface DtfStockItemDetailDto {
  item: DtfStockItemDto
  movements: DtfStockMovementDto[]
}

export const MOVEMENT_TYPE_LABEL: Record<DtfMovementType, string> = {
  1: 'Entrada',
  2: 'Saída',
  3: 'Ajuste',
}

export const MOVEMENT_TYPE_CLASS: Record<DtfMovementType, string> = {
  1: 'text-success',
  2: 'text-danger',
  3: 'text-info',
}

