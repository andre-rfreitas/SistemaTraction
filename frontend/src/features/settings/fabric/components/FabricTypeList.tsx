import { useState } from 'react'
import { Shirt } from 'lucide-react'
import { useFabricTypes } from '../hooks/useFabricTypes'
import { useDeleteFabricType } from '../hooks/useDeleteFabricType'
import { useCreateFabricColor } from '../hooks/useCreateFabricColor'
import { useDeleteFabricColor } from '../hooks/useDeleteFabricColor'
import { FabricColorForm } from './FabricColorForm'
import type { FabricTypeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'

interface Props {
  onEdit: (type: FabricTypeDto) => void
}

export function FabricTypeList({ onEdit }: Props) {
  const { data: types, isLoading, error } = useFabricTypes()
  const deleteType = useDeleteFabricType()
  const [addColorToId, setAddColorToId] = useState<string | null>(null)
  const createColor = useCreateFabricColor(addColorToId ?? '')
  const deleteColor = useDeleteFabricColor(addColorToId ?? '')

  if (isLoading)
    return (
      <div className="space-y-3">
        <Skeleton className="h-20 w-full" />
        <Skeleton className="h-20 w-full" />
        <Skeleton className="h-20 w-full" />
      </div>
    )

  if (error) return <p className="text-sm text-danger">Erro ao carregar tipos de tecido.</p>

  if (!types?.length)
    return (
      <EmptyState
        icon={Shirt}
        title="Nenhum tipo de tecido cadastrado"
        description='Clique em "+ Novo tipo" para começar.'
      />
    )

  return (
    <div className="space-y-3">
      {types.map((type) => (
        <Card key={type.id}>
          <CardContent className="pt-5 space-y-3">
            <div className="flex items-start justify-between gap-2">
              <div>
                <h3 className="font-semibold text-foreground">
                  {type.name} — {type.variation}
                </h3>
                <p className="text-sm text-muted-foreground">
                  R$ {type.pricePerKg.toFixed(2)}/kg · {type.averageKgPerRoll}kg/bobina
                  {type.averagePiecesPerRoll ? ` · ~${type.averagePiecesPerRoll} peças` : ''}
                </p>
              </div>
              <div className="flex gap-2 shrink-0">
                <Button variant="outline" size="sm" onClick={() => onEdit(type)}>
                  Editar
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => {
                    if (confirm(`Excluir "${type.name} ${type.variation}"?`)) {
                      deleteType.mutate(type.id)
                    }
                  }}
                  disabled={deleteType.isPending}
                >
                  Excluir
                </Button>
              </div>
            </div>

            <div className="flex flex-wrap gap-2 items-center">
              {type.colors.map((color) => (
                <div key={color.id} className="flex items-center gap-1 group">
                  {color.hexCode && (
                    <span
                      className="inline-block w-3 h-3 rounded-full border border-border shrink-0"
                      style={{ backgroundColor: color.hexCode }}
                    />
                  )}
                  <Badge variant="neutral" className="text-xs">
                    {color.name}
                  </Badge>
                  <button
                    className="text-xs text-muted-foreground hover:text-danger opacity-0 group-hover:opacity-100 transition-opacity leading-none"
                    title={`Remover cor ${color.name}`}
                    onClick={() => {
                      setAddColorToId(type.id)
                      deleteColor.mutate(color.id)
                    }}
                  >
                    ×
                  </button>
                </div>
              ))}

              <Dialog>
                <DialogTrigger asChild>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 text-xs text-muted-foreground hover:text-foreground"
                    onClick={() => setAddColorToId(type.id)}
                  >
                    + Cor
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>
                      Adicionar cor — {type.name} {type.variation}
                    </DialogTitle>
                  </DialogHeader>
                  <FabricColorForm
                    onSubmit={(data) => {
                      setAddColorToId(type.id)
                      createColor.mutate(data)
                    }}
                    isLoading={createColor.isPending}
                  />
                </DialogContent>
              </Dialog>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
