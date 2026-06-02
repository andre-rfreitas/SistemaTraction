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
  wp_cutter_name:          'WhatsApp — Nome do cortador',
  wp_cutter_phone:         'WhatsApp — Número do cortador',
  wp_sewer_name:           'WhatsApp — Nome do costureiro',
  wp_sewer_phone:          'WhatsApp — Número do costureiro',
  wp_dtf_name:             'WhatsApp — Nome do fornecedor DTF',
  wp_dtf_phone:            'WhatsApp — Número do fornecedor DTF',
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
    <div className="border border-border rounded-lg p-4 bg-card space-y-2">
      <div className="min-w-0">
        <p className="text-sm font-semibold text-foreground">{label}</p>
        {config.description && (
          <p className="text-xs text-muted-foreground mt-0.5">{config.description}</p>
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
        <p className="text-xs text-danger">{(upsert.error as Error).message}</p>
      )}
      {upsert.isSuccess && !isDirty && (
        <p className="text-xs text-success">Salvo ✓</p>
      )}
    </div>
  )
}
