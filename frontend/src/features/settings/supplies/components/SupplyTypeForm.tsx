import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { supplyTypeSchema, type SupplyTypeFormData } from '../schemas/supplyTypeSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

interface Props {
  defaultValues?: Partial<SupplyTypeFormData>
  isLoading: boolean
  onSubmit: (data: SupplyTypeFormData) => void
}

export function SupplyTypeForm({ defaultValues, isLoading, onSubmit }: Props) {
  const { register, handleSubmit, formState: { errors } } = useForm<SupplyTypeFormData>({
    resolver: zodResolver(supplyTypeSchema),
    defaultValues,
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <label className="text-sm font-medium text-foreground">Nome</label>
        <Input {...register('name')} placeholder="Ex: Envelope de Segurança" />
        {errors.name && <p className="text-xs text-danger">{errors.name.message}</p>}
      </div>
      <div className="space-y-1">
        <label className="text-sm font-medium text-foreground">Unidade</label>
        <Input {...register('unit')} placeholder="Ex: un, pct, rolo" />
        {errors.unit && <p className="text-xs text-danger">{errors.unit.message}</p>}
      </div>
      <div className="space-y-1">
        <label className="text-sm font-medium text-foreground">Preço por unidade (R$)</label>
        <Input
          type="number"
          step="0.01"
          min="0"
          placeholder="Ex: 0,85"
          {...register('pricePerUnit', {
            setValueAs: (v) => (v === '' || v == null ? null : parseFloat(v)),
          })}
        />
        <p className="text-xs text-muted-foreground">Usado para calcular o custo total nas entradas.</p>
        {errors.pricePerUnit && <p className="text-xs text-danger">{errors.pricePerUnit.message as string}</p>}
      </div>
      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
