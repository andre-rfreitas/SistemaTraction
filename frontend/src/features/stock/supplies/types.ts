export interface SupplyStockItemDto {
  id: string
  supplyTypeId: string
  name: string
  unit: string
  quantity: number
}

export interface SupplyStockMovementDto {
  id: string
  type: 'Entrada' | 'Saida' | 'Ajuste'
  delta: number
  reason: string | null
  createdAt: string
}

export interface RegisterSupplyMovementResult {
  movementId: string
  requiresFinancialConfirmation: boolean
  suggestedDescription: string | null
}

export interface SupplyOrderConfigDto {
  supplyTypeId: string
  name: string
  unit: string
  quantityPerOrder: number
}

export interface SupplyDeductionPreviewItem {
  supplyTypeId: string
  supplyStockItemId: string
  name: string
  unit: string
  quantityPerOrder: number
  totalQuantity: number
}
