import {
  Boxes, Layers, Scissors, ClipboardList, Wallet, Shirt, Image, Settings,
  type LucideIcon,
} from 'lucide-react'

export type TabId =
  | 'fabric' | 'rolls' | 'cutting' | 'dtf-models'
  | 'dtf-stock' | 'separation' | 'financial' | 'config'

export interface NavItem {
  id: TabId
  label: string
  icon: LucideIcon
}

export interface NavGroup {
  label: string
  items: NavItem[]
}

export const NAV_GROUPS: NavGroup[] = [
  {
    label: 'Estoque',
    items: [
      { id: 'rolls', label: 'Bobinas', icon: Layers },
      { id: 'dtf-stock', label: 'Estoque DTF', icon: Boxes },
    ],
  },
  {
    label: 'Produção',
    items: [
      { id: 'cutting', label: 'Pedidos de Corte', icon: Scissors },
      { id: 'separation', label: 'Lista de Separação', icon: ClipboardList },
    ],
  },
  {
    label: 'Financeiro',
    items: [{ id: 'financial', label: 'Financeiro', icon: Wallet }],
  },
  {
    label: 'Cadastros',
    items: [
      { id: 'fabric', label: 'Tecidos', icon: Shirt },
      { id: 'dtf-models', label: 'Modelos DTF', icon: Image },
      { id: 'config', label: 'Configurações', icon: Settings },
    ],
  },
]

export const NAV_ITEMS: NavItem[] = NAV_GROUPS.flatMap((g) => g.items)

export function findNavItem(id: TabId): NavItem {
  return NAV_ITEMS.find((i) => i.id === id) ?? NAV_ITEMS[0]
}
