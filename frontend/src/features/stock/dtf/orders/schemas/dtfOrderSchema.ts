import { z } from 'zod'

export const dtfOrderItemSchema = z.object({
  dtfModelId: z.string().uuid('Selecione um modelo'),
  sheetQuantity: z.number().int().positive('Quantidade deve ser maior que zero'),
})

export const dtfOrderSchema = z.object({
  notes: z.string().max(500).optional().nullable(),
  items: z.array(dtfOrderItemSchema).min(1, 'Adicione pelo menos um modelo'),
})

export type DtfOrderFormData = z.infer<typeof dtfOrderSchema>
export type DtfOrderItemFormData = z.infer<typeof dtfOrderItemSchema>
