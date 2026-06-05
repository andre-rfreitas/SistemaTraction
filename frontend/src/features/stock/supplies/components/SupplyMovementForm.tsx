import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { supplyMovementSchema, type SupplyMovementFormData } from '../schemas/supplyMovementSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

interface Props {
  isLoading: boolean
  onSubmit: (data: SupplyMovementFormData) => void
}

export function SupplyMovementForm({ isLoading, onSubmit }: Props) {
  const { register, handleSubmit, formState: { errors } } = useForm<SupplyMovementFormData>({
    resolver: zodResolver(supplyMovementSchema),
    defaultValues: { type: 'Entrada', quantity: 1 },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-3">
      <div className="space-y-1">
        <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Tipo</label>
        <select
          {...register('type')}
          className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm"
        >
          <option value="Entrada">Entrada</option>
          <option value="Saida">Saída</option>
          <option value="Ajuste">Ajuste</option>
        </select>
      </div>
      <div className="space-y-1">
        <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Quantidade</label>
        <Input type="number" {...register('quantity', { valueAsNumber: true })} />
        {errors.quantity && <p className="text-xs text-danger">{errors.quantity.message}</p>}
      </div>
      <div className="space-y-1">
        <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Motivo (opcional)</label>
        <Input {...register('reason')} placeholder="Ex: Compra do fornecedor X" />
      </div>
      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Registrando...' : 'Registrar'}
      </Button>
    </form>
  )
}
