import { Menu, PanelLeftClose, PanelLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from '@/components/ui/theme-toggle'

interface TopbarProps {
  title: string
  collapsed: boolean
  onToggleCollapse: () => void
  onOpenMobile: () => void
}

export function Topbar({ title, collapsed, onToggleCollapse, onOpenMobile }: TopbarProps) {
  return (
    <header className="flex h-14 items-center gap-2 border-b border-border bg-background/80 px-4 backdrop-blur">
      <Button
        variant="ghost"
        size="icon"
        className="lg:hidden"
        onClick={onOpenMobile}
        aria-label="Abrir menu"
      >
        <Menu className="size-4" />
      </Button>
      <Button
        variant="ghost"
        size="icon"
        className="hidden lg:inline-flex"
        onClick={onToggleCollapse}
        aria-label={collapsed ? 'Expandir menu' : 'Recolher menu'}
      >
        {collapsed ? <PanelLeft className="size-4" /> : <PanelLeftClose className="size-4" />}
      </Button>
      <span className="text-sm font-medium text-foreground">{title}</span>
      <div className="ml-auto">
        <ThemeToggle />
      </div>
    </header>
  )
}
