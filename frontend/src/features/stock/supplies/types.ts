export interface SupplyStockItemDto {
  id: string
  supplyTypeId: string
  name: string
  unit: string
  quantity: number
  pricePerUnit: number | null
}

export interface SupplyStockMovementDto {
  id: string
  type: 'Entrada' | 'Saida' | 'Ajuste'
  delta: number
  reason: string | null
  supplierName: string | null
  supplierPhone: string | null
  occurredAt: string
  unitPrice: number | null
  totalCost: number | null
  createdAt: string
}

export interface RegisterSupplyMovementResult {
  movementId: string
  requiresFinancialConfirmation: boolean
  suggestedDescription: string | null
  suggestedAmount: number | null
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
