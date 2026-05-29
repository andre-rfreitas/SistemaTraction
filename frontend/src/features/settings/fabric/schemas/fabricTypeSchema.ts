import { z } from 'zod'

export const fabricTypeSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100),
  variation: z.string().min(1, 'Variação é obrigatória').max(100),
  pricePerKg: z.coerce.number().positive('Preço deve ser maior que zero'),
  averageKgPerRoll: z.coerce.number().positive('Média de kg deve ser maior que zero'),
  averagePiecesPerRoll: z.preprocess((value) => {
    if (value === '' || value === null || value === undefined) {
      return undefined
    }
    return Number(value)
  }, z.number().int().positive().optional()),
})

export const fabricColorSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100),
  hexCode: z
    .string()
    .regex(/^#[0-9A-Fa-f]{6}$/, 'Formato inválido (ex: #000000)')
    .optional()
    .or(z.literal(''))
    .nullable(),
})

// Input types (o que o form recebe — strings do HTML)
export type FabricTypeFormInput = z.input<typeof fabricTypeSchema>
export type FabricColorFormInput = z.input<typeof fabricColorSchema>

// Output types (depois da transformação pelo Zod — números corretos)
export type FabricTypeFormData = z.output<typeof fabricTypeSchema>
export type FabricColorFormData = z.output<typeof fabricColorSchema>
