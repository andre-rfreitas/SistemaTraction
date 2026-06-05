import { useState } from 'react'
import { AppShell } from '@/components/layout/AppShell'
import type { TabId } from '@/components/layout/nav'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { FabricRollPage } from '@/features/fabric/rolls/FabricRollPage'
import { CuttingOrderPage } from '@/features/cutting/orders/CuttingOrderPage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'
import { DtfStockPage } from '@/features/stock/dtf/DtfStockPage'
import { SeparationListPage } from '@/features/separation/SeparationListPage'
import { FinancialPage } from '@/features/financial/FinancialPage'
import { SettingsPage } from '@/features/settings/SettingsPage'
import { ShirtStockPage } from '@/features/stock/shirts/ShirtStockPage'
import { SupplyStockPage } from '@/features/stock/supplies/SupplyStockPage'
import { SupplyTypePage } from '@/features/settings/supplies/SupplyTypePage'

function App() {
  const [tab, setTab] = useState<TabId>('financial')

  const pages: Record<TabId, React.ReactNode> = {
    fabric: <FabricTypePage />,
    rolls: <FabricRollPage />,
    cutting: <CuttingOrderPage />,
    'dtf-models': <DtfModelPage />,
    'dtf-stock': <DtfStockPage />,
    'shirt-stock': <ShirtStockPage />,
    'supply-stock': <SupplyStockPage />,
    'supply-types': <SupplyTypePage />,
    separation: <SeparationListPage />,
    financial: <FinancialPage />,
    config: <SettingsPage onNavigate={setTab} />,
  }

  return (
    <AppShell active={tab} onSelect={setTab}>
      {pages[tab]}
    </AppShell>
  )
}

export default App
