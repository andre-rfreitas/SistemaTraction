import { useState } from 'react'
import { AppShell } from '@/components/layout/AppShell'
import { useAuth } from '@/lib/auth'
import { LoginPage } from '@/features/auth/LoginPage'
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
import { SewerPage } from '@/features/sewing/sewers/SewerPage'
import { SewingOrdersPage } from '@/features/sewing/orders/SewingOrdersPage'
import { DtfOrderPage } from '@/features/stock/dtf/orders/DtfOrderPage'

function App() {
  const { isAuthenticated, isLoading } = useAuth()
  const [tab, setTab] = useState<TabId>('financial')

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-background">
        <div className="size-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <LoginPage />
  }

  const pages: Record<TabId, React.ReactNode> = {
    fabric: <FabricTypePage />,
    rolls: <FabricRollPage />,
    cutting: <CuttingOrderPage />,
    'dtf-models': <DtfModelPage />,
    'dtf-stock': <DtfStockPage />,
    'dtf-orders': <DtfOrderPage />,
    'shirt-stock': <ShirtStockPage />,
    'supply-stock': <SupplyStockPage />,
    'supply-types': <SupplyTypePage />,
    sewers: <SewerPage />,
    'sewing-orders': <SewingOrdersPage />,
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
