import { z } from 'zod'

export const cuttingOrderSchema = z.object({
  fabricRollId: z.string().min(1, 'Selecione uma bobina'),
  requestedPieces: z.record(z.string(), z.number().int().min(0)),
  notes: z.string().optional(),
})

export type CuttingOrderFormData = z.infer<typeof cuttingOrderSchema>
