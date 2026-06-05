import { useSupplyTypes } from '../hooks/useSupplyTypes'
import { useDeleteSupplyType } from '../hooks/useDeleteSupplyType'
import type { SupplyTypeDto } from '../types'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { Tag } from 'lucide-react'

interface Props {
  onEdit: (item: SupplyTypeDto) => void
}

export function SupplyTypeList({ onEdit }: Props) {
  const { data: types = [], isLoading } = useSupplyTypes()
  const deleteMutation = useDeleteSupplyType()

  if (isLoading) return <div className="space-y-2"><Skeleton className="h-12 w-full" /><Skeleton className="h-12 w-full" /></div>

  if (types.length === 0)
    return <EmptyState icon={Tag} title="Nenhum tipo de insumo cadastrado." description="Clique em '+ Novo tipo' para começar." />

  return (
    <div className="space-y-2">
      {types.map((t) => (
        <div key={t.id} className="flex items-center justify-between rounded-lg border border-border bg-card p-3">
          <div>
            <p className="text-sm font-semibold text-foreground">{t.name}</p>
            <p className="text-xs text-muted-foreground">{t.unit}</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => onEdit(t)}
              className="text-xs px-2 py-1 rounded border border-border text-muted-foreground hover:bg-muted transition-colors"
            >
              Editar
            </button>
            <button
              onClick={() => deleteMutation.mutate(t.id)}
              disabled={deleteMutation.isPending}
              className="text-xs px-2 py-1 rounded border border-danger/30 text-danger hover:bg-danger/10 transition-colors disabled:opacity-50"
            >
              Excluir
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
