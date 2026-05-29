import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  movementSchema,
  type MovementFormData,
  type MovementFormInput,
} from '../schemas/movementSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  onSubmit: (data: MovementFormData) => void
  isLoading?: boolean
}

export function RegisterMovementForm({ onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<MovementFormInput, unknown, MovementFormData>({
    resolver: zodResolver(movementSchema),
    defaultValues: { type: 1, quantity: 1, reason: '' },
  })

  const type = Number(useWatch({ control, name: 'type' }))

  return (
    <form
      onSubmit={handleSubmit((data) => {
        onSubmit(data)
        reset()
      })}
      className="space-y-4"
    >
      <div className="space-y-1">
        <Label htmlFor="type">Tipo de movimento</Label>
        <select
          id="type"
          {...register('type')}
          className="w-full border border-neutral-200 rounded-md px-3 py-2 text-sm bg-white focus:outline-none focus:ring-2 focus:ring-neutral-900"
        >
          <option value={1}>Entrada — recebimento de folhas</option>
          <option value={2}>Saída — folhas usadas em produção</option>
          <option value={3}>Ajuste — correção de inventário</option>
        </select>
        {errors.type && (
          <p className="text-sm text-red-500">{errors.type.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="quantity">
          {type === 3 ? 'Quantidade (use negativo para reduzir)' : 'Quantidade'}
        </Label>
        <Input
          id="quantity"
          type="number"
          min={type === 3 ? undefined : 1}
          step={1}
          {...register('quantity')}
        />
        {errors.quantity && (
          <p className="text-sm text-red-500">{errors.quantity.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="reason">Motivo (opcional)</Label>
        <Input id="reason" {...register('reason')} placeholder="ex: Compra #42, Produção lote 7" />
        {errors.reason && (
          <p className="text-sm text-red-500">{errors.reason.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Registrando...' : 'Registrar movimento'}
      </Button>
    </form>
  )
}
