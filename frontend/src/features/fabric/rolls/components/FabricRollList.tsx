import { Layers, Pencil, Trash2 } from 'lucide-react'
import { useFabricRolls } from '../hooks/useFabricRolls'
import type { FabricRollDto } from '../types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from '@/components/ui/table'

const STATUS_LABEL: Record<string, string> = {
  Available: 'Disponível',
  InCutting: 'Em corte',
  Consumed: 'Consumida',
}

type BadgeVariant = 'success' | 'warning' | 'neutral'

const STATUS_VARIANT: Record<string, BadgeVariant> = {
  Available: 'success',
  InCutting: 'warning',
  Consumed: 'neutral',
}

function fmt(n: number, decimals = 2) {
  return n.toLocaleString('pt-BR', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

interface Props {
  onEdit?: (roll: FabricRollDto) => void
  onDelete?: (roll: FabricRollDto) => void
}

export function FabricRollList({ onEdit, onDelete }: Props) {
  const { data: rolls = [], isLoading } = useFabricRolls()

  if (isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
        <Skeleton className="h-16 w-full" />
      </div>
    )
  }

  if (rolls.length === 0) {
    return (
      <EmptyState
        icon={Layers}
        title="Nenhuma bobina registrada"
        description='Clique em "+ Nova bobina" para começar.'
      />
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Tipo / Variação</TableHead>
          <TableHead>Cor</TableHead>
          <TableHead className="text-right">Peso (kg)</TableHead>
          <TableHead className="text-right">Preço total</TableHead>
          <TableHead className="text-right">R$/kg pago</TableHead>
          <TableHead className="text-right">R$/kg ref.</TableHead>
          <TableHead>Recebida em</TableHead>
          <TableHead>Status</TableHead>
          <TableHead />
        </TableRow>
      </TableHeader>
      <TableBody>
        {rolls.map((r) => {
          const diff = r.pricePerKgActual - r.fabricTypePricePerKg
          return (
            <TableRow key={r.id}>
              <TableCell>
                {r.fabricTypeName} — {r.fabricTypeVariation}
              </TableCell>
              <TableCell className="flex items-center gap-2">
                {r.fabricColorHexCode && (
                  <span
                    className="inline-block w-3 h-3 rounded-full border border-border"
                    style={{ backgroundColor: r.fabricColorHexCode }}
                  />
                )}
                {r.fabricColorName}
              </TableCell>
              <TableCell className="text-right tabular-nums">{fmt(r.weightKg, 3)}</TableCell>
              <TableCell className="text-right tabular-nums">R$ {fmt(r.priceTotal)}</TableCell>
              <TableCell className="text-right tabular-nums">
                <span
                  className={
                    diff > 0.001
                      ? 'text-danger font-medium'
                      : diff < -0.001
                      ? 'text-success font-medium'
                      : ''
                  }
                >
                  R$ {fmt(r.pricePerKgActual)}
                </span>
              </TableCell>
              <TableCell className="text-right tabular-nums text-muted-foreground">
                R$ {fmt(r.fabricTypePricePerKg)}
              </TableCell>
              <TableCell className="text-muted-foreground">
                {new Date(r.receivedAt).toLocaleDateString('pt-BR')}
              </TableCell>
              <TableCell>
                <Badge variant={STATUS_VARIANT[r.status] ?? 'neutral'}>
                  {STATUS_LABEL[r.status] ?? r.status}
                </Badge>
              </TableCell>
              <TableCell className="text-right">
                <div className="flex items-center justify-end gap-1">
                  {onEdit && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onEdit(r)}
                      aria-label="Editar bobina"
                    >
                      <Pencil className="w-4 h-4" />
                    </Button>
                  )}
                  {onDelete && r.status !== 'InCutting' && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onDelete(r)}
                      aria-label="Excluir bobina"
                      className="text-destructive hover:text-destructive hover:bg-destructive/10"
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          )
        })}
      </TableBody>
    </Table>
  )
}
