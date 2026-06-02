import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  financialEntrySchema,
  type FinancialEntryFormData,
  type FinancialEntryFormInput,
} from '../schemas/financialEntrySchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  onSubmit: (data: FinancialEntryFormData) => void
  isLoading?: boolean
}

export function ManualEntryForm({ onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FinancialEntryFormInput, unknown, FinancialEntryFormData>({
    resolver: zodResolver(financialEntrySchema),
    defaultValues: { type: 'Expense', category: '', amount: undefined, description: '' },
  })

  return (
    <form
      onSubmit={handleSubmit((data) => {
        onSubmit(data)
        reset()
      })}
      className="space-y-4"
    >
      <div className="space-y-1">
        <Label htmlFor="type">Tipo</Label>
        <select
          id="type"
          {...register('type')}
          className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
        >
          <option value="Expense">Despesa</option>
          <option value="Income">Receita</option>
        </select>
      </div>

      <div className="space-y-1">
        <Label htmlFor="category">Categoria</Label>
        <Input id="category" {...register('category')} placeholder="ex: Energia, Venda avulsa" />
        {errors.category && <p className="text-sm text-danger">{errors.category.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="amount">Valor (R$)</Label>
        <Input id="amount" type="number" step="0.01" min="0.01" {...register('amount')} />
        {errors.amount && <p className="text-sm text-danger">{errors.amount.message}</p>}
      </div>

      <div className="space-y-1">
        <Label htmlFor="description">Descrição</Label>
        <Input id="description" {...register('description')} placeholder="Detalhe do lançamento" />
        {errors.description && <p className="text-sm text-danger">{errors.description.message}</p>}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Lançar'}
      </Button>
    </form>
  )
}
