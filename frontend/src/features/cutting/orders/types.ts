export interface CuttingOrderDto {
  id: string
  orderNumber: number
  fabricRollId: string
  fabricTypeName: string
  fabricTypeVariation: string
  fabricColorName: string
  fabricColorHexCode: string | null
  fabricRollWeightKg: number
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

export interface RegisterSewingDeliveryResult {
  sewingDeliveryId: string
  totalGoodPieces: number
  totalDefectivePieces: number
  sewingCostTotal: number
  defectCostTotal: number
  goodPieces: Record<string, number>
  defectivePieces: Record<string, number>
}
