import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  fabricRollSchema,
  type FabricRollFormData,
  type FabricRollFormInput,
} from '../schemas/fabricRollSchema'
import { useFabricTypes } from '@/features/settings/fabric/hooks/useFabricTypes'
import type { FabricTypeDto } from '@/features/settings/fabric/types'
import { Button } from '@/components/ui/button'

interface Props {
  isLoading: boolean
  onSubmit: (data: FabricRollFormData) => void
}

export function FabricRollForm({ isLoading, onSubmit }: Props) {
  const { data: fabricTypes = [] } = useFabricTypes()
  const [selectedType, setSelectedType] = useState<FabricTypeDto | null>(null)
  const [weightKgRaw, setWeightKgRaw] = useState('')
  const [priceTotalRaw, setPriceTotalRaw] = useState('')

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FabricRollFormInput, unknown, FabricRollFormData>({
    resolver: zodResolver(fabricRollSchema),
  })

  const fabricTypeId = watch('fabricTypeId')

  useEffect(() => {
    const type = fabricTypes.find((t) => t.id === fabricTypeId) ?? null
    setSelectedType(type)
    setValue('fabricColorId', '')
  }, [fabricTypeId, fabricTypes, setValue])

  const weightKg = parseFloat(weightKgRaw) || 0
  const priceTotal = parseFloat(priceTotalRaw) || 0
  const pricePerKgPaid = weightKg > 0 && priceTotal > 0 ? priceTotal / weightKg : null
  const pricePerKgRef = selectedType?.pricePerKg ?? null
  const diff =
    pricePerKgPaid !== null && pricePerKgRef !== null ? pricePerKgPaid - pricePerKgRef : null

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-neutral-700 mb-1">
          Tipo de tecido
        </label>
        <select
          {...register('fabricTypeId')}
          className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-neutral-400"
        >
          <option value="">Selecione...</option>
          {fabricTypes.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name} — {t.variation}
            </option>
          ))}
        </select>
        {errors.fabricTypeId && (
          <p className="text-xs text-red-600 mt-1">{errors.fabricTypeId.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-neutral-700 mb-1">Cor</label>
        <select
          {...register('fabricColorId')}
          disabled={!selectedType}
          className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-neutral-400 disabled:opacity-50"
        >
          <option value="">Selecione...</option>
          {(selectedType?.colors ?? []).map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
        {errors.fabricColorId && (
          <p className="text-xs text-red-600 mt-1">{errors.fabricColorId.message}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">Peso (kg)</label>
          <input
            type="number"
            step="0.001"
            min="0.001"
            {...register('weightKg', {
              onChange: (e) => setWeightKgRaw(e.target.value),
            })}
            placeholder="Ex: 12.500"
            className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-neutral-400"
          />
          {errors.weightKg && (
            <p className="text-xs text-red-600 mt-1">{errors.weightKg.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Preço total (R$)
          </label>
          <input
            type="number"
            step="0.01"
            min="0.01"
            {...register('priceTotal', {
              onChange: (e) => setPriceTotalRaw(e.target.value),
            })}
            placeholder="Ex: 180.00"
            className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-neutral-400"
          />
          {errors.priceTotal && (
            <p className="text-xs text-red-600 mt-1">{errors.priceTotal.message}</p>
          )}
        </div>
      </div>

      {pricePerKgPaid !== null && pricePerKgRef !== null && (
        <div className="rounded-md bg-neutral-50 border border-neutral-200 p-3 text-sm space-y-1">
          <div className="flex justify-between">
            <span className="text-neutral-600">Preço/kg pago:</span>
            <span className="font-medium">R$ {pricePerKgPaid.toFixed(2)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-neutral-600">Preço/kg cadastrado:</span>
            <span className="font-medium">R$ {pricePerKgRef.toFixed(2)}</span>
          </div>
          <div className="flex justify-between border-t border-neutral-200 pt-1 mt-1">
            <span className="text-neutral-600">Diferença:</span>
            <span
              className={`font-semibold ${
                diff! > 0.005
                  ? 'text-red-600'
                  : diff! < -0.005
                  ? 'text-green-600'
                  : 'text-neutral-700'
              }`}
            >
              {diff! > 0 ? '+' : ''}R$ {diff!.toFixed(2)}
              {diff! > 0.0001
                ? ' (acima do padrão)'
                : diff! < -0.0001
                ? ' (abaixo do padrão)'
                : ''}
            </span>
          </div>
        </div>
      )}

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Registrando...' : 'Registrar chegada'}
      </Button>
    </form>
  )
}
