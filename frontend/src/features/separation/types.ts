export interface SeparationListSummary {
  id: string
  fileName: string
  uploadedAt: string
  status: 'Pending' | 'Confirmed' | 'Cancelled'
  itemCount: number
  totalQuantity: number
}

export interface SeparationItemDto {
  id: string
  sku: string
  color: string
  size: string
  quantity: number
  sortOrder: number
}

export interface SeparationListDetail {
  id: string
  fileName: string
  uploadedAt: string
  status: 'Pending' | 'Confirmed' | 'Cancelled'
  items: SeparationItemDto[]
}

export interface UpdateItemPayload {
  id: string
  sku: string
  color: string
  size: string
  quantity: number
}

export interface ShirtStockCheck {
  modelCode: string
  color: string
  size: string
  needed: number
  available: number
  ok: boolean
}

export interface StockCheckResult {
  shirtChecks: ShirtStockCheck[]
  canConfirm: boolean
}

// SKU code categories — extensible for future additions
export type SkuCodeCategory = 'Modelo' | 'Cor' | 'Tamanho'

export interface SkuCodeDto {
  id: string
  code: string
  value: string
  category: SkuCodeCategory
}

export interface SeparationConfirmResult {
  separationListId: string
  shirtDeductions: { modelCode: string; color: string; size: string; quantity: number }[]
}
