import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  dtfModelSchema,
  type DtfModelFormData,
  type DtfModelFormInput,
} from '../schemas/dtfModelSchema'
import type { DtfModelDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<DtfModelDto>
  onSubmit: (data: DtfModelFormData) => void
  isLoading?: boolean
}

export function DtfModelForm({ defaultValues, onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<DtfModelFormInput, unknown, DtfModelFormData>({
    resolver: zodResolver(dtfModelSchema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      sheetLabel: defaultValues?.sheetLabel ?? '',
      stampsPerSheet: defaultValues?.stampsPerSheet ?? undefined,
      sheetCost: defaultValues?.sheetCost ?? 49.8,
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="dtf-name">Nome do modelo</Label>
        <Input
          id="dtf-name"
          {...register('name')}
          placeholder="ex: Angel, Flying Souls..."
        />
        {errors.name && (
          <p className="text-sm text-red-500">{errors.name.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="dtf-sheetLabel">Rótulo da folha</Label>
        <Input
          id="dtf-sheetLabel"
          {...register('sheetLabel')}
          placeholder="ex: Folha 1, Folha 2..."
        />
        {errors.sheetLabel && (
          <p className="text-sm text-red-500">{errors.sheetLabel.message}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-1">
          <Label htmlFor="dtf-stampsPerSheet">Estampas por folha</Label>
          <Input
            id="dtf-stampsPerSheet"
            type="number"
            min="1"
            {...register('stampsPerSheet')}
          />
          {errors.stampsPerSheet && (
            <p className="text-sm text-red-500">{errors.stampsPerSheet.message}</p>
          )}
        </div>

        <div className="space-y-1">
          <Label htmlFor="dtf-sheetCost">Custo da folha (R$)</Label>
          <Input
            id="dtf-sheetCost"
            type="number"
            step="0.01"
            min="0.01"
            {...register('sheetCost')}
          />
          {errors.sheetCost && (
            <p className="text-sm text-red-500">{errors.sheetCost.message}</p>
          )}
        </div>
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
