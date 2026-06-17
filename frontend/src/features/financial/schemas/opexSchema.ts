import { z } from 'zod'

export const opexSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  fixedMonthlyValue: z.number().min(0, 'Valor fixo não pode ser negativo'),
  ratePercent: z.number().min(0).max(100, 'Taxa deve estar entre 0 e 100'),
  isActive: z.boolean(),
}).refine(
  (d) => d.fixedMonthlyValue > 0 || d.ratePercent > 0,
  { message: 'Informe ao menos um valor fixo ou taxa percentual', path: ['fixedMonthlyValue'] }
)

export type OpexFormData = z.infer<typeof opexSchema>
