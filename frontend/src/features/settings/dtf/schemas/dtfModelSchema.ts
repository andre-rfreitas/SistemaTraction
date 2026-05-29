import { z } from 'zod'

export const dtfModelSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100),
  sheetLabel: z.string().min(1, 'Rótulo da folha é obrigatório').max(50),
  stampsPerSheet: z.coerce
    .number({ invalid_type_error: 'Informe um número' })
    .int('Deve ser um número inteiro')
    .positive('Deve ser maior que zero'),
  sheetCost: z.coerce
    .number({ invalid_type_error: 'Informe um valor' })
    .positive('Deve ser maior que zero'),
})

export type DtfModelFormInput = z.input<typeof dtfModelSchema>
export type DtfModelFormData = z.output<typeof dtfModelSchema>
