import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { opexSchema, type OpexFormData } from '../schemas/opexSchema'
import type { OperationalExpenseDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<OpexFormData>
  isLoading: boolean
  onSubmit: (data: OpexFormData) => void
  onCancel: () => void
  editing?: OperationalExpenseDto
}

export function OpexForm({ defaultValues, isLoading, onSubmit, onCancel, editing }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<OpexFormData>({
    resolver: zodResolver(opexSchema),
    defaultValues: {
      name: editing?.name ?? defaultValues?.name ?? '',
      fixedMonthlyValue: editing?.fixedMonthlyValue ?? defaultValues?.fixedMonthlyValue ?? 0,
      ratePercent: editing?.ratePercent ?? defaultValues?.ratePercent ?? 0,
      isActive: editing?.isActive ?? defaultValues?.isActive ?? true,
    } as OpexFormData,
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="name">Nome da despesa</Label>
        <Input id="name" {...register('name')} placeholder="Ex: Energia, Folha de pagamento, Taxas TikTok" />
        {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1">
          <Label htmlFor="fixedMonthlyValue">Valor fixo mensal (R$)</Label>
          <Input
            id="fixedMonthlyValue"
            type="number"
            step="0.01"
            min="0"
            {...register('fixedMonthlyValue', { valueAsNumber: true })}
            placeholder="0,00"
          />
          <p className="text-xs text-muted-foreground">Rateado pelo período selecionado</p>
          {errors.fixedMonthlyValue && <p className="text-xs text-destructive">{errors.fixedMonthlyValue.message}</p>}
        </div>

        <div className="space-y-1">
          <Label htmlFor="ratePercent">Taxa sobre receita (%)</Label>
          <Input
            id="ratePercent"
            type="number"
            step="0.01"
            min="0"
            max="100"
            {...register('ratePercent', { valueAsNumber: true })}
            placeholder="0,00"
          />
          <p className="text-xs text-muted-foreground">% aplicado sobre receitas do período</p>
          {errors.ratePercent && <p className="text-xs text-destructive">{errors.ratePercent.message}</p>}
        </div>
      </div>

      {editing && (
        <div className="flex items-center gap-2">
          <input
            id="isActive"
            type="checkbox"
            {...register('isActive')}
            className="h-4 w-4 rounded border-border"
          />
          <Label htmlFor="isActive">Ativo</Label>
        </div>
      )}

      <div className="flex gap-2 pt-2">
        <Button type="button" variant="outline" onClick={onCancel} className="flex-1" disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" className="flex-1" disabled={isLoading}>
          {isLoading ? 'Salvando...' : editing ? 'Salvar alterações' : 'Cadastrar'}
        </Button>
      </div>
    </form>
  )
}
