import { z } from 'zod'

export const financialEntrySchema = z.object({
  type: z.enum(['Income', 'Expense']),
  category: z.string().min(1, 'Informe a categoria'),
  amount: z.coerce
    .number({ error: 'Informe o valor' })
    .positive('Valor deve ser maior que zero'),
  description: z.string().min(1, 'Informe a descrição'),
})

export type FinancialEntryFormInput = z.input<typeof financialEntrySchema>
export type FinancialEntryFormData = z.output<typeof financialEntrySchema>
