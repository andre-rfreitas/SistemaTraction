import { z } from 'zod'

export const supplyTypeSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório').max(100),
  unit: z.string().min(1, 'Unidade obrigatória').max(20),
})

export type SupplyTypeFormData = z.infer<typeof supplyTypeSchema>
