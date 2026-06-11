export interface CuttingRecommendationDto {
  colorName: string
  daysUsed: number
  basedOnOrders: number
  recommendedPieces: Record<string, number>
  demandBySize: Record<string, number>
  currentStockBySize: Record<string, number>
  hasSufficientHistory: boolean
}

export interface CuttingRecommendationHistoryItemDto {
  cuttingOrderId: string
  orderNumber: number
  fabricColorName: string
  createdAt: string
  daysUsed: number
  basedOnOrders: number
  recommendedPieces: Record<string, number>
  requestedPieces: Record<string, number>
  actualDeliveredPieces: Record<string, number> | null
}

export interface CuttingOrderItemDto {
  id: string
  fabricRollId: string
  fabricTypeName: string
  fabricTypeVariation: string
  fabricColorName: string
  fabricColorHexCode: string | null
  fabricRollWeightKg: number
  requestedPieces: Record<string, number>
  totalPieces: number
}

export interface CuttingOrderDto {
  id: string
  orderNumber: number
  items: CuttingOrderItemDto[]
  requestedPieces: Record<string, number>
  deliveredPieces: Record<string, number> | null
  totalPieces: number
  status: 'Draft' | 'SentToCutter' | 'Delivered' | 'SewingDelivered'
  sentAt: string | null
  notes: string | null
  createdAt: string
}

export interface CreateCuttingOrderResult {
  cuttingOrderId: string
  orderNumber: number
  whatsAppMessage: string
  waMeLink: string | null
  cutterPhone: string
  cutterName: string
}

export interface SendCuttingOrderResult {
  sent: boolean
  waMeLink: string | null
  error: string | null
}

export interface RegisterCuttingDeliveryResult {
  cuttingDeliveryId: string
  totalPieces: number
  cuttingCostTotal: number
  whatsAppMessage: string
  waMeLink: string | null
  sewerPhone: string
  sewerName: string
}

export interface SewingItemResult {
  fabricColorName: string
  fabricTypeName: string
  fabricColorHexCode: string | null
  goodPieces: Record<string, number>
  defectivePieces: Record<string, number>
}

export interface RegisterSewingDeliveryResult {
  sewingDeliveryId: string
  totalGoodPieces: number
  totalDefectivePieces: number
  sewingCostTotal: number
  defectCostTotal: number
  goodPieces: Record<string, number>
  defectivePieces: Record<string, number>
  items: SewingItemResult[]
}
