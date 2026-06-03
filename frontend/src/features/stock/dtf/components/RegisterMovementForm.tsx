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
  stampsPerSheet: number
  onSubmit: (data: MovementFormData) => void
  isLoading?: boolean
}

export function RegisterMovementForm({ stampsPerSheet, onSubmit, isLoading }: Props) {
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
  const quantity = Number(useWatch({ control, name: 'quantity' })) || 0
  const isEntrada = type === 1

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
          className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
        >
          <option value={1}>Entrada — recebimento de folhas</option>
          <option value={2}>Saída — estampas usadas em produção</option>
          <option value={3}>Ajuste — correção de inventário (estampas)</option>
        </select>
        {errors.type && (
          <p className="text-sm text-danger">{errors.type.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="quantity">
          {isEntrada
            ? 'Folhas recebidas'
            : type === 3
              ? 'Estampas (use negativo para reduzir)'
              : 'Estampas'}
        </Label>
        <Input
          id="quantity"
          type="number"
          min={type === 3 ? undefined : 1}
          step={1}
          {...register('quantity')}
        />
        {isEntrada && quantity > 0 && (
          <p className="text-xs text-muted-foreground">
            = {quantity * stampsPerSheet} estampas ({stampsPerSheet} por folha)
          </p>
        )}
        {errors.quantity && (
          <p className="text-sm text-danger">{errors.quantity.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="reason">Motivo (opcional)</Label>
        <Input id="reason" {...register('reason')} placeholder="ex: Compra #42, Produção lote 7" />
        {errors.reason && (
          <p className="text-sm text-danger">{errors.reason.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Registrando...' : 'Registrar movimento'}
      </Button>
    </form>
  )
}
