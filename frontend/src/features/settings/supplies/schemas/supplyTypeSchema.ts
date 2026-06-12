import { z } from 'zod'

export const supplyTypeSchema = z
  .object({
    name: z.string().min(1, 'Nome obrigatório').max(100),
    unit: z.string().min(1, 'Unidade obrigatória').max(20),
    pricePerUnit: z
      .number()
      .positive('Preço deve ser maior que zero')
      .optional()
      .nullable(),
    yieldBasis: z.enum(['None', 'PerOrder', 'PerProduct']).default('None'),
    yieldQuantity: z.number().positive('Rendimento deve ser maior que zero').optional().nullable(),
    yieldProductName: z.string().max(100).optional().nullable(),
  })
  .superRefine((data, ctx) => {
    if (data.yieldBasis !== 'None') {
      if (!data.yieldQuantity || data.yieldQuantity <= 0) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Quantidade do rendimento deve ser maior que zero',
          path: ['yieldQuantity'],
        })
      }
    }
    if (data.yieldBasis === 'PerProduct') {
      if (!data.yieldProductName?.trim()) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Selecione um produto',
          path: ['yieldProductName'],
        })
      }
    }
  })

export type SupplyTypeFormData = z.infer<typeof supplyTypeSchema>
