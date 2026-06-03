import { useState, useMemo } from 'react'
import { useAppConfigs } from '../config/hooks/useAppConfigs'
import { useUpsertAppConfig } from '../config/hooks/useUpsertAppConfig'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import type { AppConfigDto } from '../config/types'

type TemplateDef = {
  key: string
  label: string
  variables: string[]
  sampleVars: Record<string, string>
}

const TEMPLATES: TemplateDef[] = [
  {
    key: 'wp_template_cutter',
    label: 'Mensagem para o Cortador',
    variables: ['{OrderNumber}', '{Color}', '{Variation}', '{SizesBlock}', '{Total}'],
    sampleVars: { OrderNumber: '42', Color: 'Pretas', Variation: 'Regular', SizesBlock: '20 P\n20 M\n20 G\n16 G1', Total: '76' },
  },
  {
    key: 'wp_template_sewer',
    label: 'Mensagem para o Costureiro',
    variables: ['{OrderNumber}', '{Color}', '{Variation}', '{Total}', '{SizesBlock}', '{Cost}'],
    sampleVars: { OrderNumber: '42', Color: 'Pretas', Variation: 'Regular', Total: '76', SizesBlock: '20 P\n20 M\n20 G\n16 G1', Cost: '76,00' },
  },
  {
    key: 'wp_template_dtf',
    label: 'Mensagem para o Fornecedor DTF',
    variables: ['{Date}', '{SheetsBlock}', '{TotalSheets}', '{TotalCost}'],
    sampleVars: {
      Date: new Date().toLocaleDateString('pt-BR'),
      SheetsBlock: 'Folha 1 — Angel (9 estampas/folha) — 1 folha',
      TotalSheets: '1',
      TotalCost: '49,80',
    },
  },
]

function applyTemplate(template: string, vars: Record<string, string>): string {
  return Object.entries(vars).reduce(
    (t, [k, v]) => t.replace(`{${k}}`, v),
    template
  )
}

function TemplateRow({ config, label, variables, sampleVars }: {
  config: AppConfigDto | undefined
  label: string
  variables: string[]
  sampleVars: Record<string, string>
}) {
  const serverValue = config?.value ?? ''
  const [value, setValue] = useState(serverValue)
  const upsert = useUpsertAppConfig()

  const isDirty = value !== serverValue
  const preview = useMemo(() => applyTemplate(value, sampleVars), [value, sampleVars])

  if (!config) return null

  return (
    <div className="rounded-lg border border-border bg-card p-4 space-y-3">
      <p className="text-sm font-semibold text-foreground">{label}</p>
      <div className="flex flex-wrap gap-1">
        {variables.map((v) => (
          <code key={v} className="rounded bg-muted px-1.5 py-0.5 text-[11px] font-mono text-muted-foreground">{v}</code>
        ))}
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <p className="text-xs text-muted-foreground mb-1">Template</p>
          <textarea
            value={value}
            onChange={(e) => setValue(e.target.value)}
            rows={5}
            spellCheck={false}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm font-mono text-foreground resize-y shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
        <div>
          <p className="text-xs text-muted-foreground mb-1">Preview (dados de exemplo)</p>
          <pre className="w-full rounded-md border border-border bg-muted/50 px-3 py-2 text-sm font-mono text-foreground whitespace-pre-wrap min-h-[116px]">
            {preview}
          </pre>
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Button size="sm" disabled={!isDirty || upsert.isPending} onClick={() => upsert.mutate({ key: config.key, value })}>
          {upsert.isPending ? 'Salvando...' : 'Salvar template'}
        </Button>
        {upsert.isSuccess && !isDirty && <span className="text-xs text-success">Salvo ✓</span>}
        {upsert.isError && <span className="text-xs text-destructive">Erro ao salvar</span>}
      </div>
    </div>
  )
}

export function TemplatesSection() {
  const { data: configs = [], isLoading } = useAppConfigs()

  if (isLoading) return <div className="space-y-3">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-56 w-full" />)}</div>

  return (
    <div className="space-y-5">
      <p className="text-sm text-muted-foreground">
        Personalize as mensagens enviadas via WhatsApp. As variáveis são substituídas pelos dados reais ao enviar.
      </p>
      {TEMPLATES.map(({ key, label, variables, sampleVars }) => (
        <TemplateRow
          key={key}
          config={configs.find((c) => c.key === key)}
          label={label}
          variables={variables}
          sampleVars={sampleVars}
        />
      ))}
    </div>
  )
}
