import { useState } from 'react'
import type { AppConfigDto } from '../types'
import { useUpsertAppConfig } from '../hooks/useUpsertAppConfig'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

const CONFIG_LABELS: Record<string, string> = {
  'dtf.alerta_estoque_minimo':       'Alerta de estoque mínimo DTF (folhas)',
  'dtf.custo_folha_padrao':          'Custo padrão por folha DTF (R$)',
  'estoque.tecido.alerta_minimo_kg': 'Alerta de estoque mínimo de tecido (kg)',
  'pedido.lead_time_padrao_dias':    'Lead time padrão de pedidos (dias)',
  'sistema.moeda':                   'Moeda do sistema',
  'sistema.timezone':                'Fuso horário',
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
