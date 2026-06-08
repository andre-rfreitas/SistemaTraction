import { useState } from 'react'
import { CheckCircle, XCircle } from 'lucide-react'
import { useAppConfigs } from '../config/hooks/useAppConfigs'
import { useUpsertAppConfig } from '../config/hooks/useUpsertAppConfig'
import { useShopifyStatus } from '@/features/financial/hooks/useShopifyStatus'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'

export function ShopifySection() {
  const { data: configs = [], isLoading } = useAppConfigs()
  const status = useShopifyStatus()
  const upsertUrl = useUpsertAppConfig()
  const upsertToken = useUpsertAppConfig()

  const urlConfig = configs.find((c) => c.key === 'shopify_store_url')
  const tokenConfig = configs.find((c) => c.key === 'shopify_access_token')

  const [urlVal, setUrlVal] = useState(urlConfig?.value ?? '')
  const [tokenVal, setTokenVal] = useState(tokenConfig?.value ?? '')

  // Sync local state when configs load
  if (urlConfig && urlVal === '' && urlConfig.value !== '') setUrlVal(urlConfig.value)
  if (tokenConfig && tokenVal === '' && tokenConfig.value !== '') setTokenVal(tokenConfig.value)

  const urlDirty = urlVal !== (urlConfig?.value ?? '')
  const tokenDirty = tokenVal !== (tokenConfig?.value ?? '')

  function save() {
    if (urlDirty && urlConfig) upsertUrl.mutate({ key: urlConfig.key, value: urlVal })
    if (tokenDirty && tokenConfig) upsertToken.mutate({ key: tokenConfig.key, value: tokenVal })
  }

  const isSaving = upsertUrl.isPending || upsertToken.isPending
  const isDirty = urlDirty || tokenDirty

  if (isLoading) {
    return (
      <div className="space-y-3">
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="rounded-lg border border-border bg-card p-5 space-y-5">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h3 className="text-sm font-semibold text-foreground">Credenciais da API</h3>
            <p className="text-xs text-muted-foreground mt-0.5">
              Configure a integração com sua loja Shopify para importar pedidos como receitas
              financeiras.
            </p>
          </div>
          {status.data && (
            <div className="shrink-0 flex items-center gap-1.5">
              {status.data.configured ? (
                <>
                  <CheckCircle className="h-4 w-4 text-success" />
                  <span className="text-xs text-success font-medium">Configurado</span>
                </>
              ) : (
                <>
                  <XCircle className="h-4 w-4 text-muted-foreground" />
                  <span className="text-xs text-muted-foreground">Não configurado</span>
                </>
              )}
            </div>
          )}
        </div>

        <div className="space-y-4">
          <div className="space-y-1.5">
            <label className="text-xs font-medium text-foreground">URL da loja</label>
            <Input
              type="text"
              value={urlVal}
              onChange={(e) => setUrlVal(e.target.value)}
              placeholder="minha-loja.myshopify.com"
              className="font-mono"
            />
            <p className="text-xs text-muted-foreground">Sem https:// — só o domínio</p>
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-medium text-foreground">Access Token</label>
            <Input
              type="password"
              value={tokenVal}
              onChange={(e) => setTokenVal(e.target.value)}
              placeholder="shpat_••••••••••••••••"
              className="font-mono"
            />
          </div>
        </div>

        <div className="flex items-center gap-3">
          <Button
            size="sm"
            disabled={!isDirty || isSaving}
            isLoading={isSaving}
            onClick={save}
          >
            Salvar e testar conexão
          </Button>
          {(upsertUrl.isSuccess || upsertToken.isSuccess) && !isDirty && (
            <span className="text-xs text-success">Salvo ✓</span>
          )}
          {(upsertUrl.isError || upsertToken.isError) && (
            <span className="text-xs text-danger">Erro ao salvar</span>
          )}
        </div>

        {status.data?.lastSync && (
          <p className="text-xs text-muted-foreground border-t border-border pt-3">
            Último sync:{' '}
            <span className="font-medium text-foreground">
              {new Date(status.data.lastSync).toLocaleString('pt-BR')}
            </span>{' '}
            — {status.data.lastSyncImported} pedido(s) importado(s)
          </p>
        )}
      </div>

      <div className="rounded-lg border border-border bg-muted/30 p-4 space-y-2">
        <p className="text-xs font-semibold text-foreground">Como gerar o token de acesso</p>
        <ol className="text-xs text-muted-foreground space-y-1 list-decimal list-inside">
          <li>Acesse o Shopify Admin da sua loja</li>
          <li>Vá em <strong className="text-foreground">Configurações → Apps e canais de vendas</strong></li>
          <li>Clique em <strong className="text-foreground">Desenvolver apps</strong></li>
          <li>Crie um novo app e acesse <strong className="text-foreground">Permissões de API do Admin</strong></li>
          <li>Habilite o escopo <code className="bg-muted px-1 rounded">read_orders</code></li>
          <li>Instale o app e copie o <strong className="text-foreground">Token de acesso da API do Admin</strong></li>
        </ol>
      </div>
    </div>
  )
}
