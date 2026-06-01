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
  dtfModelId: string | null
  dtfModelName: string | null
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
  dtfModelId: string | null
}

export interface ShirtStockCheck {
  color: string
  size: string
  needed: number
  available: number
  ok: boolean
}

export interface DtfStockCheck {
  dtfModelId: string
  modelName: string
  sheetLabel: string
  stampsPerSheet: number
  sheetCost: number
  needed: number
  available: number
  fromStock: boolean
  sheetsToOrder: number
  stampsFromSheets: number
  surplus: number
  orderCost: number
}

export interface StockCheckResult {
  shirtChecks: ShirtStockCheck[]
  dtfChecks: DtfStockCheck[]
  totalDtfCost: number
  canConfirm: boolean
}

export type SkuCodeCategory = 'Modelo' | 'EstampaDtf' | 'Cor' | 'Tamanho'

export interface SkuCodeDto {
  id: string
  code: string
  value: string
  category: SkuCodeCategory
  dtfModelId: string | null
}

export interface SeparationConfirmResult {
  separationListId: string
  shirtDeductions: { color: string; size: string; quantity: number }[]
  dtfUsages: { modelName: string; quantityUsed: number }[]
  dtfOrders: {
    modelName: string
    sheetLabel: string
    stampsPerSheet: number
    sheetsOrdered: number
    sheetCost: number
    totalCost: number
  }[]
  totalDtfCost: number
  whatsAppMessage: string | null
  waMeLink: string | null
  dtfSupplierName: string
  dtfSupplierPhone: string
}
