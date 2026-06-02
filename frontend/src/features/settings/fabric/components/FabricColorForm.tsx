import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  fabricColorSchema,
  type FabricColorFormData,
  type FabricColorFormInput,
} from '../schemas/fabricTypeSchema'
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
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FabricColorFormInput>({
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
        {errors.name && <p className="text-sm text-danger">{errors.name.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="hexCode">Código hex (opcional)</Label>
        <div className="flex flex-wrap gap-2 items-center">
          <Input
            id="hexCode"
            {...register('hexCode')}
            placeholder="#000000"
            className="font-mono"
          />
          <Controller
            name="hexCode"
            control={control}
            render={({ field }) => {
              const pickerValue = /^#[0-9A-Fa-f]{6}$/.test(field.value ?? '')
                ? (field.value ?? '#000000')
                : '#000000'

              return (
                <Input
                  id="hexPicker"
                  name={field.name}
                  type="color"
                  value={pickerValue}
                  onChange={(event) => field.onChange(event.target.value)}
                  onBlur={field.onBlur}
                  className="h-9 w-14 p-0"
                />
              )
            }}
          />
        </div>
        <p className="text-sm text-muted-foreground">Selecione a cor com o mouse ou informe o código hex manualmente.</p>
        {errors.hexCode && <p className="text-sm text-danger">{errors.hexCode.message}</p>}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar cor'}
      </Button>
    </form>
  )
}
