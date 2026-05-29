import { useState } from 'react'
import type { AppConfigDto } from '../types'
import { useUpsertAppConfig } from '../hooks/useUpsertAppConfig'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

const CONFIG_LABELS: Record<string, string> = {
  cutting_price_default:   'Preço de corte por peça (R$)',
  sewing_price_default:    'Preço de costura — P / M / G / GG (R$)',
  sewing_price_g1:         'Preço de costura — G1 (R$)',
  dtf_sheet_price_default: 'Preço por folha DTF (R$)',
  stock_alert_threshold:   'Alerta de estoque mínimo (unidades)',
  recommendation_days:     'Dias de histórico para recomendação de corte',
  sizes_available:         'Tamanhos disponíveis (separados por vírgula)',
}

interface Props {
  config: AppConfigDto
}

export function AppConfigRow({ config }: Props) {
  const [value, setValue] = useState(config.value)
  const upsert = useUpsertAppConfig()

  const isDirty = value !== config.value
  const label = CONFIG_LABELS[config.key] ?? config.key

  function handleSave() {
    upsert.mutate({ key: config.key, value }, { onSuccess: () => {} })
  }

  return (
    <div className="border rounded-lg p-4 bg-white space-y-2">
      <div className="min-w-0">
        <p className="text-sm font-semibold text-neutral-800">{label}</p>
        {config.description && (
          <p className="text-xs text-neutral-500 mt-0.5">{config.description}</p>
        )}
      </div>
      <div className="flex gap-2">
        <Input
          value={value}
          onChange={(e) => setValue(e.target.value)}
          className="font-mono text-sm"
        />
        <Button
          size="sm"
          disabled={!isDirty || upsert.isPending}
          onClick={handleSave}
        >
          {upsert.isPending ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
      {upsert.isError && (
        <p className="text-xs text-red-500">{(upsert.error as Error).message}</p>
      )}
      {upsert.isSuccess && !isDirty && (
        <p className="text-xs text-green-600">Salvo ✓</p>
      )}
    </div>
  )
}
