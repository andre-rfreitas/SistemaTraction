import { useState } from 'react'
import { AppShell } from '@/components/layout/AppShell'
import type { TabId } from '@/components/layout/nav'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { FabricRollPage } from '@/features/fabric/rolls/FabricRollPage'
import { CuttingOrderPage } from '@/features/cutting/orders/CuttingOrderPage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'
import { DtfStockPage } from '@/features/stock/dtf/DtfStockPage'
import { AppConfigPage } from '@/features/settings/config/AppConfigPage'
import { SeparationListPage } from '@/features/separation/SeparationListPage'
import { FinancialPage } from '@/features/financial/FinancialPage'

const PAGES: Record<TabId, React.ReactNode> = {
  fabric: <FabricTypePage />,
  rolls: <FabricRollPage />,
  cutting: <CuttingOrderPage />,
  'dtf-models': <DtfModelPage />,
  'dtf-stock': <DtfStockPage />,
  separation: <SeparationListPage />,
  financial: <FinancialPage />,
  config: <AppConfigPage />,
}

function App() {
  const [tab, setTab] = useState<TabId>('financial')
  return (
    <AppShell active={tab} onSelect={setTab}>
      {PAGES[tab]}
    </AppShell>
  )
}

export default App
