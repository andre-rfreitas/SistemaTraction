import { useForm, useFieldArray, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { dtfOrderSchema, type DtfOrderFormData } from '../schemas/dtfOrderSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import type { DtfOrderDto } from '../types'

interface DtfModelOption {
  id: string
  name: string
  sheetLabel: string
  stampsPerSheet: number
}

interface Props {
  models: DtfModelOption[]
  defaultValues?: DtfOrderDto
  isLoading: boolean
  onSubmit: (data: DtfOrderFormData) => void
}

export function DtfOrderForm({ models, defaultValues, isLoading, onSubmit }: Props) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<DtfOrderFormData>({
    resolver: zodResolver(dtfOrderSchema),
    defaultValues: defaultValues
      ? {
          notes: defaultValues.notes ?? '',
          items: defaultValues.items
            .filter(i => i.dtfModelId != null)
            .map(i => ({
              dtfModelId: i.dtfModelId as string,
              sheetQuantity: i.sheetQuantity,
            })),
        }
      : { notes: '', items: [{ dtfModelId: '', sheetQuantity: 1 }] },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })
  const watchedItems = useWatch({ control, name: 'items' })

  function getModelById(id: string) {
    return models.find(m => m.id === id)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <label className="text-sm font-medium text-foreground">Observações</label>
        <textarea
          {...register('notes')}
          rows={2}
          placeholder="Opcional"
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring resize-none"
        />
        {errors.notes && <p className="text-xs text-danger">{errors.notes.message}</p>}
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Modelos</label>
        {fields.map((field, index) => {
          const selectedModelId = watchedItems?.[index]?.dtfModelId
          const selectedModel = selectedModelId ? getModelById(selectedModelId) : undefined
          const qty = watchedItems?.[index]?.sheetQuantity ?? 0

          return (
            <div key={field.id} className="flex items-start gap-2">
              <div className="flex-1 space-y-1">
                <select
                  {...register(`items.${index}.dtfModelId`)}
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <option value="">Selecionar modelo...</option>
                  {models.map(m => (
                    <option key={m.id} value={m.id}>
                      {m.name} ({m.sheetLabel})
                    </option>
                  ))}
                </select>
                {errors.items?.[index]?.dtfModelId && (
                  <p className="text-xs text-danger">{errors.items[index].dtfModelId?.message}</p>
                )}
              </div>

              <div className="w-24 space-y-1">
                <Input
                  type="number"
                  min="1"
                  step="1"
                  placeholder="Folhas"
                  {...register(`items.${index}.sheetQuantity`, {
                    valueAsNumber: true,
                  })}
                />
                {errors.items?.[index]?.sheetQuantity && (
                  <p className="text-xs text-danger">{errors.items[index].sheetQuantity?.message}</p>
                )}
              </div>

              {selectedModel && qty > 0 && (
                <div className="pt-2 text-xs text-muted-foreground whitespace-nowrap">
                  = {qty * selectedModel.stampsPerSheet} est.
                </div>
              )}

              <button
                type="button"
                onClick={() => remove(index)}
                className="pt-2 text-xs text-danger hover:underline"
              >
                Remover
              </button>
            </div>
          )
        })}

        {errors.items?.message && (
          <p className="text-xs text-danger">{errors.items.message}</p>
        )}

        <button
          type="button"
          onClick={() => append({ dtfModelId: '', sheetQuantity: 1 })}
          className="text-xs text-primary hover:underline"
        >
          + Adicionar modelo
        </button>
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
