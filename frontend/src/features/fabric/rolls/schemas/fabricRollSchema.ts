import { z } from 'zod'

export const fabricRollSchema = z.object({
  fabricTypeId: z.string().min(1, 'Selecione o tipo de tecido'),
  fabricColorId: z.string().min(1, 'Selecione a cor'),
  weightKg: z.coerce
    .number({ error: 'Informe o peso' })
    .positive('Peso deve ser maior que zero'),
  priceTotal: z.coerce
    .number({ error: 'Informe o preço' })
    .positive('Preço deve ser maior que zero'),
})

export type FabricRollFormInput = z.input<typeof fabricRollSchema>
export type FabricRollFormData = z.output<typeof fabricRollSchema>
