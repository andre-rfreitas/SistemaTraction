import { useDtfModels } from '../hooks/useDtfModels'
import { useDeleteDtfModel } from '../hooks/useDeleteDtfModel'
import type { DtfModelDto } from '../types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'

interface Props {
  onEdit: (model: DtfModelDto) => void
}

export function DtfModelList({ onEdit }: Props) {
  const { data: models, isLoading, error } = useDtfModels()
  const deleteModel = useDeleteDtfModel()

  if (isLoading)
    return <p className="text-neutral-500 text-sm">Carregando...</p>
  if (error)
    return <p className="text-red-500 text-sm">Erro ao carregar modelos DTF.</p>
  if (!models?.length)
    return (
      <p className="text-neutral-500 text-sm">
        Nenhum modelo DTF cadastrado. Clique em "+ Novo modelo" para começar.
      </p>
    )

  return (
    <div className="space-y-3">
      {models.map((model) => (
        <div
          key={model.id}
          className="border rounded-lg p-4 bg-white flex items-center justify-between gap-4"
        >
          <div className="flex items-center gap-3 min-w-0">
            <Badge variant="outline" className="shrink-0 font-mono text-xs">
              {model.sheetLabel}
            </Badge>
            <div className="min-w-0">
              <h3 className="font-semibold text-neutral-900 truncate">
                {model.name}
              </h3>
              <p className="text-sm text-neutral-500">
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
