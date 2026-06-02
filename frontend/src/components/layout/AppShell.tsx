import { useState } from 'react'
import { Dialog, DialogContent } from '@/components/ui/dialog'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'
import { findNavItem, type TabId } from './nav'
import { cn } from '@/lib/utils'

interface AppShellProps {
  active: TabId
  onSelect: (id: TabId) => void
  children: React.ReactNode
}

export function AppShell({ active, onSelect, children }: AppShellProps) {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem('sidebar-collapsed') === '1')
  const [mobileOpen, setMobileOpen] = useState(false)

  const toggleCollapse = () => {
    setCollapsed((c) => {
      const next = !c
      localStorage.setItem('sidebar-collapsed', next ? '1' : '0')
      return next
    })
  }

  const handleSelect = (id: TabId) => {
    onSelect(id)
    setMobileOpen(false)
  }

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <aside
        className={cn(
          'hidden shrink-0 border-r border-border bg-card transition-[width] duration-200 lg:block',
          collapsed ? 'w-16' : 'w-60',
        )}
      >
        <div className="flex h-14 items-center gap-2 border-b border-border px-4">
          <div className="size-6 rounded bg-primary" />
          {!collapsed && <span className="text-sm font-semibold tracking-tight">StockShirt</span>}
        </div>
        <Sidebar active={active} onSelect={handleSelect} collapsed={collapsed} />
      </aside>

      <Dialog open={mobileOpen} onOpenChange={setMobileOpen}>
        <DialogContent className="left-0 top-0 h-screen max-w-[15rem] translate-x-0 translate-y-0 rounded-none border-y-0 border-l-0 p-0">
          <div className="flex h-14 items-center border-b border-border px-4">
            <span className="text-sm font-semibold tracking-tight">StockShirt</span>
          </div>
          <Sidebar active={active} onSelect={handleSelect} />
        </DialogContent>
      </Dialog>

      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar
          title={findNavItem(active).label}
          collapsed={collapsed}
          onToggleCollapse={toggleCollapse}
          onOpenMobile={() => setMobileOpen(true)}
        />
        <main className="flex-1 overflow-y-auto">
          <div className="mx-auto w-full max-w-7xl px-4 py-6 sm:px-6 lg:px-8">{children}</div>
        </main>
      </div>
    </div>
  )
}
