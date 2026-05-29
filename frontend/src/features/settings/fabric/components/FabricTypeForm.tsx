import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { fabricTypeSchema, type FabricTypeFormData, type FabricTypeFormInput } from '../schemas/fabricTypeSchema'
import type { FabricTypeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<FabricTypeDto>
  onSubmit: (data: FabricTypeFormData) => void
  isLoading?: boolean
}

export function FabricTypeForm({ defaultValues, onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FabricTypeFormInput, unknown, FabricTypeFormData>({
    resolver: zodResolver(fabricTypeSchema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      variation: defaultValues?.variation ?? '',
      pricePerKg: defaultValues?.pricePerKg ?? undefined,
      averageKgPerRoll: defaultValues?.averageKgPerRoll ?? undefined,
      averagePiecesPerRoll: defaultValues?.averagePiecesPerRoll ?? undefined,
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="name">Nome do tecido</Label>
        <Input id="name" {...register('name')} placeholder="ex: Malha" />
        {errors.name && <p className="text-sm text-red-500">{errors.name.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="variation">Variação</Label>
        <Input id="variation" {...register('variation')} placeholder="ex: Regular, Premium" />
        {errors.variation && <p className="text-sm text-red-500">{errors.variation.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="pricePerKg">Preço por kg (R$)</Label>
        <Input id="pricePerKg" type="number" step="0.01" min="0" {...register('pricePerKg')} />
        {errors.pricePerKg && <p className="text-sm text-red-500">{errors.pricePerKg.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="averageKgPerRoll">Média de kg por bobina</Label>
        <Input id="averageKgPerRoll" type="number" step="0.1" min="0" {...register('averageKgPerRoll')} />
        {errors.averageKgPerRoll && (
          <p className="text-sm text-red-500">{errors.averageKgPerRoll.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="averagePiecesPerRoll">Peças por bobina (opcional)</Label>
        <Input id="averagePiecesPerRoll" type="number" min="1" {...register('averagePiecesPerRoll')} />
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
