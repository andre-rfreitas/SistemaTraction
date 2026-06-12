import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useShirtStock } from './hooks/useShirtStock'
import { useAdjustShirtStock } from './hooks/useAdjustShirtStock'
import { ShirtStockGrid } from './components/ShirtStockGrid'
import { ShirtStockMovementsTable } from './components/ShirtStockMovementsTable'
import type { ShirtStockGridDto } from './types'

type DraftGrid = Record<string, Record<string, number>>

function buildDraftGrid(data: ShirtStockGridDto): DraftGrid {
  return Object.fromEntries(
    data.rows.map((row) => [
      row.colorId,
      Object.fromEntries(data.sizes.map((s) => [s, row.quantities[s] ?? 0])),
    ])
  )
}

export function ShirtStockPage() {
  const [isEditMode, setIsEditMode] = useState(false)
  const [draftGrid, setDraftGrid] = useState<DraftGrid>({})
  const [isSaving, setIsSaving] = useState(false)

  const { data, isLoading } = useShirtStock()
  const adjust = useAdjustShirtStock()
  const queryClient = useQueryClient()

  function enterEditMode() {
    if (!data) return
    setDraftGrid(buildDraftGrid(data))
    setIsEditMode(true)
  }

  function cancelEditMode() {
    setIsEditMode(false)
    setDraftGrid({})
  }

  function handleQuantityChange(colorId: string, size: string, value: number) {
    setDraftGrid((prev) => ({
      ...prev,
      [colorId]: { ...prev[colorId], [size]: value },
    }))
  }

  async function handleConfirm() {
    if (!data) return

    const payloads = []
    for (const row of data.rows) {
      const draftRow = draftGrid[row.colorId] ?? {}
      for (const size of data.sizes) {
        const originalQty = row.quantities[size] ?? 0
        const newQty = draftRow[size] ?? originalQty
        const delta = newQty - originalQty
        if (delta === 0) continue
        payloads.push({
          fabricColorId: row.colorId,
          size,
          adjustmentType: (delta > 0 ? 'Entrada' : 'Saída') as 'Entrada' | 'Saída',
          quantity: Math.abs(delta),
          reason: 'Ajuste manual',
        })
      }
    }

    if (payloads.length === 0) {
      cancelEditMode()
      return
    }

    setIsSaving(true)
    try {
      await Promise.all(payloads.map((p) => adjust.mutateAsync(p)))
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock-movements'] })
    } finally {
      setIsSaving(false)
      setIsEditMode(false)
      setDraftGrid({})
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Estoque de Camisetas"
        description={
          isEditMode
            ? 'Edite as quantidades diretamente na tabela e confirme para salvar.'
            : 'Visão geral do estoque por cor e tamanho. Ajustes manuais ficam registrados no histórico.'
        }
        actions={
          isEditMode ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={cancelEditMode} disabled={isSaving}>
                Cancelar
              </Button>
              <Button onClick={handleConfirm} disabled={isSaving}>
                {isSaving ? 'Salvando...' : 'Confirmar edições'}
              </Button>
            </div>
          ) : (
            <Button variant="outline" onClick={enterEditMode} disabled={!data || isLoading}>
              Ajuste manual
            </Button>
          )
        }
      />

      {isLoading ? (
        <Skeleton className="h-48 w-full rounded-lg" />
      ) : data ? (
        <ShirtStockGrid
          data={data}
          editMode={isEditMode}
          draftQuantities={isEditMode ? draftGrid : undefined}
          onQuantityChange={handleQuantityChange}
        />
      ) : null}

      {!isEditMode && (
        <div className="space-y-3">
          <h2 className="text-sm font-semibold text-foreground">Histórico de movimentações</h2>
          <ShirtStockMovementsTable />
        </div>
      )}
    </div>
  )
}
