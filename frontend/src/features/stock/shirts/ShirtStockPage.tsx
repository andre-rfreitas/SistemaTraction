import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { useShirtStock } from './hooks/useShirtStock'
import { useAdjustShirtStock } from './hooks/useAdjustShirtStock'
import { ShirtStockGrid } from './components/ShirtStockGrid'
import { ShirtStockMovementsTable } from './components/ShirtStockMovementsTable'
import type { ShirtStockGridDto } from './types'

type ProductCategory = 'camisetas' | 'toucas' | 'meias' | 'calcas'
type ShirtSubType = 'regular' | 'over'

const CATEGORIES: { id: ProductCategory; label: string }[] = [
  { id: 'camisetas', label: 'Camisetas' },
  { id: 'toucas', label: 'Toucas' },
  { id: 'meias', label: 'Meias' },
  { id: 'calcas', label: 'Calças' },
]

const SHIRT_TYPES: { id: ShirtSubType; label: string }[] = [
  { id: 'regular', label: 'Regular' },
  { id: 'over', label: 'Over' },
]

type DraftGrid = Record<string, Record<string, number>>

function buildDraftGrid(data: ShirtStockGridDto): DraftGrid {
  return Object.fromEntries(
    data.rows.map((row) => [
      row.colorId,
      Object.fromEntries(data.sizes.map((s) => [s, row.quantities[s] ?? 0])),
    ])
  )
}

function EmptyProductState({ label }: { label: string }) {
  return (
    <div className="rounded-lg border border-border bg-card p-12 text-center text-sm text-muted-foreground">
      Estoque de <span className="font-medium text-foreground">{label}</span> em breve.
    </div>
  )
}

export function ShirtStockPage() {
  const [category, setCategory] = useState<ProductCategory>('camisetas')
  const [shirtType, setShirtType] = useState<ShirtSubType>('regular')
  const [isEditMode, setIsEditMode] = useState(false)
  const [draftGrid, setDraftGrid] = useState<DraftGrid>({})
  const [isSaving, setIsSaving] = useState(false)

  const { data, isLoading } = useShirtStock()
  const adjust = useAdjustShirtStock()
  const queryClient = useQueryClient()

  function handleCategoryChange(cat: ProductCategory) {
    setCategory(cat)
    setIsEditMode(false)
    setDraftGrid({})
  }

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

  const categoryLabel = CATEGORIES.find((c) => c.id === category)?.label ?? ''

  return (
    <div className="space-y-6">
      <PageHeader
        title="Produtos"
        description={
          isEditMode
            ? 'Edite as quantidades diretamente na tabela e confirme para salvar.'
            : 'Visão geral do estoque por categoria de produto.'
        }
        actions={
          category === 'camisetas' && !isEditMode ? (
            <Button variant="outline" onClick={enterEditMode} disabled={!data || isLoading}>
              Ajuste manual
            </Button>
          ) : category === 'camisetas' && isEditMode ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={cancelEditMode} disabled={isSaving}>
                Cancelar
              </Button>
              <Button onClick={handleConfirm} disabled={isSaving}>
                {isSaving ? 'Salvando...' : 'Confirmar edições'}
              </Button>
            </div>
          ) : null
        }
      />

      {/* Abas de categoria */}
      <div className="flex gap-0 border-b border-border">
        {CATEGORIES.map((cat) => (
          <button
            key={cat.id}
            type="button"
            onClick={() => handleCategoryChange(cat.id)}
            className={cn(
              'px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px',
              category === cat.id
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground',
            )}
          >
            {cat.label}
          </button>
        ))}
      </div>

      {category === 'camisetas' ? (
        <div className="space-y-4">
          {/* Sub-abas Regular / Over */}
          <div className="flex gap-1">
            {SHIRT_TYPES.map((type) => (
              <button
                key={type.id}
                type="button"
                onClick={() => { setShirtType(type.id); setIsEditMode(false); setDraftGrid({}) }}
                className={cn(
                  'rounded-md px-3 py-1.5 text-xs font-medium transition-colors',
                  shirtType === type.id
                    ? 'bg-accent/10 text-accent'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                )}
              >
                {type.label}
              </button>
            ))}
          </div>

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
      ) : (
        <EmptyProductState label={categoryLabel} />
      )}
    </div>
  )
}
