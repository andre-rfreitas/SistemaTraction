import { z } from 'zod'

export const supplyMovementSchema = z.object({
  type: z.enum(['Entrada', 'Saida', 'Ajuste']),
  quantity: z.number().int(),
  reason: z.string().optional(),
})

export type SupplyMovementFormData = z.infer<typeof supplyMovementSchema>
