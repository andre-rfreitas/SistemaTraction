import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { fabricColorSchema, type FabricColorFormData } from '../schemas/fabricTypeSchema'
import type { FabricColorDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<FabricColorDto>
  onSubmit: (data: FabricColorFormData) => void
  isLoading?: boolean
}

export function FabricColorForm({ defaultValues, onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FabricColorFormData>({
    resolver: zodResolver(fabricColorSchema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      hexCode: defaultValues?.hexCode ?? '',
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="colorName">Nome da cor</Label>
        <Input id="colorName" {...register('name')} placeholder="ex: Preto" />
        {errors.name && <p className="text-sm text-red-500">{errors.name.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="hexCode">Código hex (opcional)</Label>
        <div className="flex gap-2 items-center">
          <Input
            id="hexCode"
            {...register('hexCode')}
            placeholder="#000000"
            className="font-mono"
          />
        </div>
        {errors.hexCode && <p className="text-sm text-red-500">{errors.hexCode.message}</p>}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar cor'}
      </Button>
    </form>
  )
}
