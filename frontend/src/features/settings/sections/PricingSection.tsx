import { useState } from 'react'
import { useAppConfigs } from '../config/hooks/useAppConfigs'
import { useUpsertAppConfig } from '../config/hooks/useUpsertAppConfig'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import type { AppConfigDto } from '../config/types'

const PRICING_KEYS = [
  { key: 'cutting_price_default', label: 'Corte por peça (R$)', hint: 'Preço pago ao cortador por peça. Aplicado a todos os tamanhos.' },
  { key: 'sewing_price_default', label: 'Costura P/M/G/GG (R$)', hint: 'Preço pago ao costureiro por peça nos tamanhos P, M, G e GG.' },
  { key: 'sewing_price_g1', label: 'Costura G1 (R$)', hint: 'Preço por peça G1, que tem custo diferenciado.' },
  { key: 'dtf_sheet_price_default', label: 'Folha DTF (R$)', hint: 'Preço padrão de uma folha DTF ao fornecedor.' },
]

function PricingRow({ config, label, hint }: { config: AppConfigDto | undefined; label: string; hint: string }) {
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
      <div className="flex items-center gap-2">
        <Input
          type="number"
          step="0.01"
          min="0"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          className="font-mono max-w-[160px]"
        />
        <Button size="sm" disabled={!isDirty || upsert.isPending} onClick={() => upsert.mutate({ key: config.key, value })}>
          {upsert.isPending ? 'Salvando...' : 'Salvar'}
        </Button>
        {upsert.isSuccess && !isDirty && <span className="text-xs text-success">Salvo ✓</span>}
      </div>
    </div>
  )
}

export function PricingSection() {
  const { data: configs = [], isLoading } = useAppConfigs()

  if (isLoading) return <div className="space-y-3">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}</div>

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Estes valores são usados nos lançamentos financeiros automáticos ao registrar entregas.
      </p>
      {PRICING_KEYS.map(({ key, label, hint }) => (
        <PricingRow key={key} config={configs.find((c) => c.key === key)} label={label} hint={hint} />
      ))}
    </div>
  )
}
