import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { supplyTypeSchema, type SupplyTypeFormData } from '../schemas/supplyTypeSchema'
import { useSewerProductTypeNames } from '../hooks/useSewerProductTypeNames'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

interface Props {
  defaultValues?: Partial<SupplyTypeFormData>
  isLoading: boolean
  onSubmit: (data: SupplyTypeFormData) => void
}

export function SupplyTypeForm({ defaultValues, isLoading, onSubmit }: Props) {
  const { register, handleSubmit, setValue, control, formState: { errors } } = useForm<SupplyTypeFormData>({
    resolver: zodResolver(supplyTypeSchema),
    defaultValues: {
      yieldBasis: 'None',
      yieldQuantity: null,
      yieldProductName: null,
      ...defaultValues,
    },
  })

  const yieldBasis = useWatch({ control, name: 'yieldBasis' })
  const { data: productNames = [] } = useSewerProductTypeNames({ enabled: yieldBasis === 'PerProduct' })


  const currentYieldProductName = useWatch({ control, name: 'yieldProductName' })

  const allProductNames = currentYieldProductName && !productNames.includes(currentYieldProductName)
    ? [currentYieldProductName, ...productNames]
    : productNames

  function handleRemoveYield() {
    setValue('yieldBasis', 'None')
    setValue('yieldQuantity', null)
    setValue('yieldProductName', null)
  }

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

      {/* Seção Rendimento */}
      <div className="rounded-md border border-border p-3 space-y-3">
        <div className="flex items-center justify-between">
          <label className="text-sm font-medium text-foreground">Rendimento</label>
          {yieldBasis !== 'None' && (
            <button
              type="button"
              onClick={handleRemoveYield}
              className="text-xs text-danger hover:underline"
            >
              Remover
            </button>
          )}
        </div>

        {yieldBasis === 'None' ? (
          <button
            type="button"
            onClick={() => { setValue('yieldBasis', 'PerOrder'); setValue('yieldQuantity', 1) }}
            className="text-xs text-primary hover:underline"
          >
            + Configurar rendimento
          </button>
        ) : (
          <div className="space-y-2">
            <p className="text-xs text-muted-foreground">1 unidade rende:</p>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                step="0.01"
                min="0.01"
                className="w-24"
                {...register('yieldQuantity', {
                  setValueAs: (v) => (v === '' || v == null ? null : parseFloat(v)),
                })}
              />
              <select
                {...register('yieldBasis', {
                  onChange: (e) => {
                    if (e.target.value !== 'PerProduct') setValue('yieldProductName', null)
                  },
                })}
                className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                <option value="PerOrder">Pedido</option>
                <option value="PerAllProducts">Todos os produtos</option>
                <option value="PerProduct">Produto específico</option>
              </select>
              {yieldBasis === 'PerProduct' && (
                <select
                  {...register('yieldProductName')}
                  className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <option value="">Selecionar produto...</option>
                  {allProductNames.map((name) => (
                    <option key={name} value={name}>{name}</option>
                  ))}
                </select>
              )}
            </div>
            {errors.yieldQuantity && (
              <p className="text-xs text-danger">{errors.yieldQuantity.message}</p>
            )}
            {errors.yieldProductName && (
              <p className="text-xs text-danger">{errors.yieldProductName.message as string}</p>
            )}
            {yieldBasis === 'PerProduct' && allProductNames.length === 0 && (
              <p className="text-xs text-muted-foreground">
                Nenhum produto encontrado. Cadastre tipos de produto nas costureiras primeiro.
              </p>
            )}
          </div>
        )}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
