import {
  Boxes, Layers, Scissors, ClipboardList, Wallet, Shirt, Image, Settings, Archive, Package, Tag,
  Spool, Users,
  type LucideIcon,
} from 'lucide-react'

export type TabId =
  | 'fabric' | 'rolls' | 'cutting' | 'dtf-models'
  | 'dtf-stock' | 'shirt-stock' | 'supply-stock' | 'separation' | 'financial' | 'config'
  | 'supply-types' | 'sewing-orders' | 'sewers'

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
    label: 'Produção',
    items: [
      { id: 'rolls', label: 'Bobinas', icon: Layers },
      { id: 'cutting', label: 'Pedidos de Corte', icon: Scissors },
      { id: 'sewing-orders', label: 'Pedidos de Costura', icon: Spool },
      { id: 'separation', label: 'Lista de Separação', icon: ClipboardList },
    ],
  },
  {
    label: 'Estoque',
    items: [
      { id: 'shirt-stock', label: 'Camisetas', icon: Archive },
      { id: 'supply-stock', label: 'Insumos e Embalagens', icon: Package },
      { id: 'dtf-stock', label: 'Estoque DTF', icon: Boxes },
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
      { id: 'sewers', label: 'Costureiras', icon: Users },
      { id: 'dtf-models', label: 'Modelos DTF', icon: Image },
      { id: 'supply-types', label: 'Tipos de Insumo', icon: Tag },
      { id: 'config', label: 'Configurações', icon: Settings },
    ],
  },
]

export const NAV_ITEMS: NavItem[] = NAV_GROUPS.flatMap((g) => g.items)

export function findNavItem(id: TabId): NavItem {
  return NAV_ITEMS.find((i) => i.id === id) ?? NAV_ITEMS[0]
}
