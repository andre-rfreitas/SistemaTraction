export type DtfOrderStatus = 'Draft' | 'Sent' | 'Received'

export interface DtfOrderItemDto {
  id: string
  dtfModelId: string | null
  modelName: string
  sheetLabel: string
  sheetQuantity: number
  stampsPerSheet: number
  stampsTotal: number
}

export interface DtfOrderDto {
  id: string
  orderNumber: number
  status: DtfOrderStatus
  notes: string | null
  sentAt: string | null
  receivedAt: string | null
  items: DtfOrderItemDto[]
  createdAt: string
}
