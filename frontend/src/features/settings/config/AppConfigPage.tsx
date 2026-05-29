import { useAppConfigs } from './hooks/useAppConfigs'
import { AppConfigRow } from './components/AppConfigRow'

export function AppConfigPage() {
  const { data: configs, isLoading, error } = useAppConfigs()

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-bold text-neutral-900">Configurações do sistema</h2>
        <p className="text-sm text-neutral-500">
          Parâmetros globais do sistema. Edite o valor e clique em Salvar.
        </p>
      </div>

      {isLoading && <p className="text-sm text-neutral-500">Carregando...</p>}
      {error && <p className="text-sm text-red-500">Erro ao carregar configurações.</p>}

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
