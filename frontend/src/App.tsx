import { useState } from 'react'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'

type Tab = 'fabric' | 'dtf'

function App() {
  const [tab, setTab] = useState<Tab>('fabric')

  return (
    <main className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-neutral-900 mb-6">
        SistemaTraction — Configurações
      </h1>

      {/* Tab navigation */}
      <div className="flex gap-1 mb-8 border-b border-neutral-200">
        <button
          id="tab-fabric"
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px ${
            tab === 'fabric'
              ? 'border-neutral-900 text-neutral-900'
              : 'border-transparent text-neutral-500 hover:text-neutral-700'
          }`}
          onClick={() => setTab('fabric')}
        >
          Tecidos
        </button>
        <button
          id="tab-dtf"
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px ${
            tab === 'dtf'
              ? 'border-neutral-900 text-neutral-900'
              : 'border-transparent text-neutral-500 hover:text-neutral-700'
          }`}
          onClick={() => setTab('dtf')}
        >
          Estampas DTF
        </button>
      </div>

      {tab === 'fabric' && <FabricTypePage />}
      {tab === 'dtf' && <DtfModelPage />}
    </main>
  )
}

export default App
