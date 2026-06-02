import { useDtfModels } from '../hooks/useDtfModels'
import { useDeleteDtfModel } from '../hooks/useDeleteDtfModel'
import type { DtfModelDto } from '../types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { Image } from 'lucide-react'

interface Props {
  onEdit: (model: DtfModelDto) => void
}

export function DtfModelList({ onEdit }: Props) {
  const { data: models, isLoading, error } = useDtfModels()
  const deleteModel = useDeleteDtfModel()

  if (isLoading)
    return (
      <div className="space-y-3">
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
      </div>
    )
  if (error)
    return <p className="text-sm text-danger">Erro ao carregar modelos DTF.</p>
  if (!models?.length)
    return (
      <EmptyState
        icon={Image}
        title="Nenhum modelo DTF cadastrado"
        description='Clique em "+ Novo modelo" para começar.'
      />
    )

  return (
    <div className="space-y-3">
      {models.map((model) => (
        <div
          key={model.id}
          className="border border-border rounded-lg p-4 bg-card flex items-center justify-between gap-4"
        >
          <div className="flex items-center gap-3 min-w-0">
            <Badge variant="outline" className="shrink-0 font-mono text-xs">
              {model.sheetLabel}
            </Badge>
            <div className="min-w-0">
              <h3 className="font-semibold text-foreground truncate">
                {model.name}
              </h3>
              <p className="text-sm text-muted-foreground">
                {model.stampsPerSheet} estampas/folha · R${' '}
                {model.sheetCost.toFixed(2)}/folha
              </p>
            </div>
          </div>

          <div className="flex gap-2 shrink-0">
            <Button variant="outline" size="sm" onClick={() => onEdit(model)}>
              Editar
            </Button>
            <Button
              variant="destructive"
              size="sm"
              disabled={deleteModel.isPending}
              onClick={() => {
                if (confirm(`Excluir o modelo "${model.name}"?`)) {
                  deleteModel.mutate(model.id)
                }
              }}
            >
              Excluir
            </Button>
          </div>
        </div>
      ))}
    </div>
  )
}
