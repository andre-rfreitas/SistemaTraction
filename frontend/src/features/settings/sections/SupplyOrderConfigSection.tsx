import { useState } from 'react'
import { useSupplyOrderConfigs } from '../config/hooks/useSupplyOrderConfigs'
import { useUpsertSupplyOrderConfig } from '../config/hooks/useUpsertSupplyOrderConfig'
import { Skeleton } from '@/components/ui/skeleton'

export function SupplyOrderConfigSection() {
  const { data: configs = [], isLoading } = useSupplyOrderConfigs()
  const upsert = useUpsertSupplyOrderConfig()
  const [saved, setSaved] = useState<string | null>(null)

  function handleChange(supplyTypeId: string, value: string) {
    const qty = parseInt(value)
    if (isNaN(qty) || qty < 0) return
    upsert.mutate(
      { supplyTypeId, quantityPerOrder: qty },
      {
        onSuccess: () => {
          setSaved(supplyTypeId)
          setTimeout(() => setSaved(null), 1500)
        },
      }
    )
  }

  if (isLoading) return <div className="space-y-2"><Skeleton className="h-10 w-full" /><Skeleton className="h-10 w-full" /></div>

  if (configs.length === 0)
    return <p className="text-sm text-muted-foreground">Nenhum tipo de insumo cadastrado. Crie em Cadastros → Tipos de Insumo.</p>

  return (
    <div className="space-y-3">
      <p className="text-xs text-muted-foreground">
        Configure quantas unidades de cada insumo são descontadas por pedido ao confirmar uma lista de separação.
      </p>
      {configs.map((c) => (
        <div key={c.supplyTypeId} className="flex items-center justify-between gap-4 rounded-lg border border-border bg-card p-3">
          <div>
            <p className="text-sm font-medium text-foreground">{c.name}</p>
            <p className="text-xs text-muted-foreground">{c.unit} por pedido</p>
          </div>
          <div className="flex items-center gap-2">
            <input
              type="number"
              min="0"
              defaultValue={c.quantityPerOrder}
              onBlur={(e) => handleChange(c.supplyTypeId, e.target.value)}
              className="w-20 rounded-md border border-input bg-background px-3 py-1.5 text-sm text-center focus:outline-none focus:ring-2 focus:ring-ring"
            />
            {saved === c.supplyTypeId && (
              <span className="text-xs text-success">✓</span>
            )}
          </div>
        </div>
      ))}
    </div>
  )
}
