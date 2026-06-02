import { useAppConfigs } from './hooks/useAppConfigs'
import { AppConfigRow } from './components/AppConfigRow'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'

export function AppConfigPage() {
  const { data: configs, isLoading, error } = useAppConfigs()

  return (
    <div className="space-y-6">
      <PageHeader
        title="Configurações do sistema"
        description="Parâmetros globais do sistema. Edite o valor e clique em Salvar."
      />

      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20 w-full rounded-lg" />
          ))}
        </div>
      )}
      {error && <p className="text-sm text-danger">Erro ao carregar configurações.</p>}

      {configs && (
        <div className="space-y-3">
          {configs.map((cfg) => (
            <AppConfigRow key={cfg.key} config={cfg} />
          ))}
        </div>
      )}
    </div>
  )
}
