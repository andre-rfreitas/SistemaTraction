import { useState } from 'react'
import { useAppConfigs } from '../config/hooks/useAppConfigs'
import { useUpsertAppConfig } from '../config/hooks/useUpsertAppConfig'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import type { AppConfigDto } from '../config/types'

const CONTACTS = [
  { nameKey: 'wp_cutter_name', phoneKey: 'wp_cutter_phone', label: 'Cortador' },
  { nameKey: 'wp_sewer_name', phoneKey: 'wp_sewer_phone', label: 'Costureiro' },
  { nameKey: 'wp_dtf_name', phoneKey: 'wp_dtf_phone', label: 'Fornecedor DTF' },
]

function ContactRow({ label, nameConfig, phoneConfig }: {
  label: string
  nameConfig: AppConfigDto | undefined
  phoneConfig: AppConfigDto | undefined
}) {
  const [name, setName] = useState(nameConfig?.value ?? '')
  const [phone, setPhone] = useState(phoneConfig?.value ?? '')
  const upsert = useUpsertAppConfig()

  const nameDirty = name !== (nameConfig?.value ?? '')
  const phoneDirty = phone !== (phoneConfig?.value ?? '')
  const isDirty = nameDirty || phoneDirty

  function save() {
    if (nameDirty && nameConfig) upsert.mutate({ key: nameConfig.key, value: name })
    if (phoneDirty && phoneConfig) upsert.mutate({ key: phoneConfig.key, value: phone })
  }

  return (
    <div className="rounded-lg border border-border bg-card p-4 space-y-3">
      <p className="text-sm font-semibold text-foreground">{label}</p>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="text-xs text-muted-foreground mb-1 block">Nome</label>
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder={label} />
        </div>
        <div>
          <label className="text-xs text-muted-foreground mb-1 block">Número (com DDI)</label>
          <Input value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="5511999999999" className="font-mono" />
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Button size="sm" disabled={!isDirty || upsert.isPending} onClick={save}>
          {upsert.isPending ? 'Salvando...' : 'Salvar'}
        </Button>
        {upsert.isSuccess && !isDirty && <span className="text-xs text-success">Salvo ✓</span>}
      </div>
    </div>
  )
}

function NicochatFields({ configs }: { configs: AppConfigDto[] }) {
  const upsertUrl = useUpsertAppConfig()
  const upsertKey = useUpsertAppConfig()

  const urlConfig = configs.find((c) => c.key === 'wp_nicochat_url')
  const keyConfig = configs.find((c) => c.key === 'wp_nicochat_key')

  const [urlVal, setUrlVal] = useState(urlConfig?.value ?? '')
  const [keyVal, setKeyVal] = useState(keyConfig?.value ?? '')

  const urlDirty = urlVal !== (urlConfig?.value ?? '')
  const keyDirty = keyVal !== (keyConfig?.value ?? '')

  return (
    <div className="space-y-3 pt-2">
      <p className="text-xs text-muted-foreground">Configure as credenciais da API Nicochat abaixo.</p>
      <div>
        <label className="text-xs text-muted-foreground mb-1 block">URL da API</label>
        <div className="flex gap-2">
          <Input type="url" value={urlVal} onChange={(e) => setUrlVal(e.target.value)} placeholder="https://api.nicochat.com" className="font-mono" />
          <Button size="sm" disabled={!urlDirty || upsertUrl.isPending} onClick={() => urlConfig && upsertUrl.mutate({ key: urlConfig.key, value: urlVal })}>Salvar</Button>
          {upsertUrl.isSuccess && !urlDirty && <span className="text-xs text-success self-center">✓</span>}
        </div>
      </div>
      <div>
        <label className="text-xs text-muted-foreground mb-1 block">API Key</label>
        <div className="flex gap-2">
          <Input type="password" value={keyVal} onChange={(e) => setKeyVal(e.target.value)} placeholder="••••••••••••••••" className="font-mono" />
          <Button size="sm" disabled={!keyDirty || upsertKey.isPending} onClick={() => keyConfig && upsertKey.mutate({ key: keyConfig.key, value: keyVal })}>Salvar</Button>
          {upsertKey.isSuccess && !keyDirty && <span className="text-xs text-success self-center">✓</span>}
        </div>
      </div>
    </div>
  )
}

export function WhatsAppSection() {
  const { data: configs = [], isLoading } = useAppConfigs()
  const upsert = useUpsertAppConfig()

  const providerConfig = configs.find((c) => c.key === 'wp_provider')
  const provider = providerConfig?.value ?? 'manual'

  if (isLoading) return <div className="space-y-3">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-32 w-full" />)}</div>

  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <h3 className="text-sm font-semibold text-foreground">Destinatários</h3>
        {CONTACTS.map(({ nameKey, phoneKey, label }) => (
          <ContactRow
            key={label}
            label={label}
            nameConfig={configs.find((c) => c.key === nameKey)}
            phoneConfig={configs.find((c) => c.key === phoneKey)}
          />
        ))}
      </div>

      <div className="space-y-3">
        <h3 className="text-sm font-semibold text-foreground">Provedor de envio</h3>
        <div className="rounded-lg border border-border bg-card p-4 space-y-4">
          <div className="flex gap-3">
            {(['manual', 'nicochat'] as const).map((p) => (
              <button
                key={p}
                onClick={() => providerConfig && upsert.mutate({ key: 'wp_provider', value: p })}
                className={`flex-1 rounded-md border py-2.5 text-sm font-medium transition-colors ${
                  provider === p
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-border bg-background text-muted-foreground hover:border-primary/40'
                }`}
              >
                {p === 'manual' ? 'Manual (wa.me)' : 'Nicochat API'}
              </button>
            ))}
          </div>
          {provider === 'manual' && (
            <p className="text-xs text-muted-foreground">
              As mensagens são enviadas via links wa.me que abrem o WhatsApp Web. Nenhuma integração necessária.
            </p>
          )}
          {provider === 'nicochat' && <NicochatFields configs={configs} />}
        </div>
      </div>
    </div>
  )
}
