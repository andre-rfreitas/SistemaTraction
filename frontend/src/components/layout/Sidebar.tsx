import { NAV_GROUPS, type TabId } from './nav'
import { cn } from '@/lib/utils'

interface SidebarProps {
  active: TabId
  onSelect: (id: TabId) => void
  collapsed?: boolean
}

export function Sidebar({ active, onSelect, collapsed = false }: SidebarProps) {
  return (
    <nav className="flex min-h-0 flex-1 flex-col gap-5 overflow-y-auto p-3" aria-label="Navegação principal">
      {NAV_GROUPS.map((group) => (
        <div key={group.label} className="space-y-1">
          {!collapsed && (
            <p className="px-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
              {group.label}
            </p>
          )}
          {group.items.map((item) => {
            const Icon = item.icon
            const isActive = item.id === active
            return (
              <button
                key={item.id}
                type="button"
                onClick={() => onSelect(item.id)}
                aria-current={isActive ? 'page' : undefined}
                title={collapsed ? item.label : undefined}
                className={cn(
                  'flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-sm font-medium transition-colors',
                  collapsed && 'justify-center',
                  isActive
                    ? 'bg-accent/10 text-accent'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                )}
              >
                <Icon className="size-4 shrink-0" />
                {!collapsed && <span className="truncate">{item.label}</span>}
              </button>
            )
          })}
        </div>
      ))}
    </nav>
  )
}
