import { useState } from 'react'
import { Shirt, Image, DollarSign, MessageSquare, FileText, Package, ChevronRight, Tag } from 'lucide-react'
import { PageHeader } from '@/components/ui/page-header'
import { PricingSection } from './sections/PricingSection'
import { WhatsAppSection } from './sections/WhatsAppSection'
import { TemplatesSection } from './sections/TemplatesSection'
import { StockSection } from './sections/StockSection'
import { SupplyOrderConfigSection } from './sections/SupplyOrderConfigSection'
import type { TabId } from '@/components/layout/nav'

type SettingsTab = 'pricing' | 'whatsapp' | 'templates' | 'stock' | 'embalagens'

const TABS: { id: SettingsTab; label: string; icon: React.ElementType }[] = [
  { id: 'pricing', label: 'Preços', icon: DollarSign },
  { id: 'whatsapp', label: 'WhatsApp', icon: MessageSquare },
  { id: 'templates', label: 'Templates', icon: FileText },
  { id: 'stock', label: 'Estoque', icon: Package },
  { id: 'embalagens', label: 'Embalagens', icon: Tag },
]

interface Props {
  onNavigate: (tab: TabId) => void
}

export function SettingsPage({ onNavigate }: Props) {
  const [active, setActive] = useState<SettingsTab>('pricing')

  return (
    <div className="space-y-6">
      <PageHeader
        title="Configurações"
        description="Parâmetros de produção, contatos WhatsApp, templates e estoque."
      />

      {/* Quick links */}
      <div className="grid grid-cols-2 gap-3">
        <QuickLink
          icon={Shirt}
          label="Tipos de Tecido"
          description="Gerenciar tipos, variações e cores"
          onClick={() => onNavigate('fabric')}
        />
        <QuickLink
          icon={Image}
          label="Modelos DTF"
          description="Gerenciar estampas e folhas DTF"
          onClick={() => onNavigate('dtf-models')}
        />
        <QuickLink
          icon={Tag}
          label="Tipos de Insumo"
          description="Gerenciar embalagens e materiais"
          onClick={() => onNavigate('supply-types')}
        />
      </div>

      {/* Tab bar */}
      <div className="flex gap-1 border-b border-border">
        {TABS.map(({ id, label, icon: Icon }) => (
          <button
            key={id}
            onClick={() => setActive(id)}
            className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium transition-colors border-b-2 -mb-px ${
              active === id
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            <Icon className="h-4 w-4" />
            {label}
          </button>
        ))}
      </div>

      {/* Section content */}
      <div>
        {active === 'pricing' && <PricingSection />}
        {active === 'whatsapp' && <WhatsAppSection />}
        {active === 'templates' && <TemplatesSection />}
        {active === 'stock' && <StockSection />}
        {active === 'embalagens' && <SupplyOrderConfigSection />}
      </div>
    </div>
  )
}

function QuickLink({
  icon: Icon,
  label,
  description,
  onClick,
}: {
  icon: React.ElementType
  label: string
  description: string
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      className="flex items-center gap-3 rounded-lg border border-border bg-card p-4 text-left transition-colors hover:border-primary/40 hover:bg-primary/5 group"
    >
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-muted group-hover:bg-primary/10">
        <Icon className="h-4 w-4 text-muted-foreground group-hover:text-primary" />
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-sm font-semibold text-foreground">{label}</p>
        <p className="text-xs text-muted-foreground">{description}</p>
      </div>
      <ChevronRight className="h-4 w-4 text-muted-foreground shrink-0" />
    </button>
  )
}
