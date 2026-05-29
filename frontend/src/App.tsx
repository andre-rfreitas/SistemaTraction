import { useState } from 'react'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'
import { DtfStockPage } from '@/features/stock/dtf/DtfStockPage'
import { AppConfigPage } from '@/features/settings/config/AppConfigPage'

type Tab = 'fabric' | 'dtf-models' | 'dtf-stock' | 'config'

const TABS: { id: Tab; label: string }[] = [
  { id: 'fabric',     label: 'Tecidos' },
  { id: 'dtf-models', label: 'Modelos DTF' },
  { id: 'dtf-stock',  label: 'Estoque DTF' },
  { id: 'config',     label: 'Configurações' },
]

function App() {
  const [tab, setTab] = useState<Tab>('fabric')

  return (
    <main className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-neutral-900 mb-6">
        SistemaTraction
      </h1>

      <div className="flex gap-1 mb-8 border-b border-neutral-200">
        {TABS.map(({ id, label }) => (
          <button
            key={id}
            onClick={() => setTab(id)}
            className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px ${
              tab === id
                ? 'border-neutral-900 text-neutral-900'
                : 'border-transparent text-neutral-500 hover:text-neutral-700'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'fabric'     && <FabricTypePage />}
      {tab === 'dtf-models' && <DtfModelPage />}
      {tab === 'dtf-stock'  && <DtfStockPage />}
      {tab === 'config'     && <AppConfigPage />}
    </main>
  )
}

export default App
