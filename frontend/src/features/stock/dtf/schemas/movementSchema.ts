import { z } from 'zod'

export const movementSchema = z
  .object({
    type: z.coerce.number().int().min(1).max(3),
    quantity: z.coerce.number().int(),
    reason: z.string().max(500).optional().or(z.literal('')),
  })
  .superRefine((data, ctx) => {
    if (data.type === 1 || data.type === 2) {
      if (data.quantity <= 0) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['quantity'],
          message: 'Quantidade deve ser maior que zero para Entrada e Saída.',
        })
      }
    }
    if (data.type === 3 && data.quantity === 0) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['quantity'],
        message: 'Ajuste não pode ser zero.',
      })
    }
  })

export type MovementFormInput = z.input<typeof movementSchema>
export type MovementFormData = z.output<typeof movementSchema>
