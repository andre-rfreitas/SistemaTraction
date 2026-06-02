import { useState } from 'react'
import { useAppConfigs } from '../config/hooks/useAppConfigs'
import { useUpsertAppConfig } from '../config/hooks/useUpsertAppConfig'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import type { AppConfigDto } from '../config/types'

const STOCK_KEYS = [
  {
    key: 'stock_alert_threshold',
    label: 'Alerta de estoque mínimo',
    hint: 'Alerta visual quando qualquer combinação cor × tamanho atingir este valor ou menos.',
    unit: 'unidades',
    type: 'number',
    min: '0',
  },
  {
    key: 'sizes_available',
    label: 'Tamanhos disponíveis',
    hint: 'Lista de tamanhos em ordem, separados por vírgula. Ex: P,M,G,G1,GG',
    unit: '',
    type: 'text',
    min: undefined,
  },
  {
    key: 'recommendation_days',
    label: 'Período de histórico para recomendação',
    hint: 'Número de dias de histórico usado para calcular a recomendação de corte.',
    unit: 'dias',
    type: 'number',
    min: '1',
  },
]

function StockRow({ config, label, hint, unit, type, min }: {
  config: AppConfigDto | undefined
  label: string
  hint: string
  unit: string
  type: string
  min: string | undefined
}) {
  const [value, setValue] = useState(config?.value ?? '')
  const upsert = useUpsertAppConfig()
  const isDirty = value !== (config?.value ?? '')

  if (!config) return null

  return (
    <div className="rounded-lg border border-border bg-card p-4 space-y-2">
      <div>
        <p className="text-sm font-semibold text-foreground">{label}</p>
        <p className="text-xs text-muted-foreground">{hint}</p>
      </div>
      <div className="flex items-center gap-2 flex-wrap">
        <Input
          type={type}
          min={min}
          value={value}
          onChange={(e) => setValue(e.target.value)}
          className={`font-mono ${type === 'number' ? 'max-w-[140px]' : 'max-w-sm'}`}
        />
        {unit && <span className="text-xs text-muted-foreground">{unit}</span>}
        <Button size="sm" disabled={!isDirty || upsert.isPending} onClick={() => upsert.mutate({ key: config.key, value })}>
          {upsert.isPending ? 'Salvando...' : 'Salvar'}
        </Button>
        {upsert.isSuccess && !isDirty && <span className="text-xs text-success">Salvo ✓</span>}
      </div>
      {config.key === 'sizes_available' && value && (
        <div className="flex flex-wrap gap-1 pt-1">
          {value.split(',').filter(Boolean).map((s) => (
            <span key={s} className="rounded bg-primary/10 px-2 py-0.5 text-xs font-mono font-semibold text-primary">
              {s.trim()}
            </span>
          ))}
        </div>
      )}
    </div>
  )
}

export function StockSection() {
  const { data: configs = [], isLoading } = useAppConfigs()

  if (isLoading) return <div className="space-y-3">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}</div>

  return (
    <div className="space-y-4">
      {STOCK_KEYS.map(({ key, label, hint, unit, type, min }) => (
        <StockRow key={key} config={configs.find((c) => c.key === key)} label={label} hint={hint} unit={unit} type={type} min={min} />
      ))}
    </div>
  )
}
