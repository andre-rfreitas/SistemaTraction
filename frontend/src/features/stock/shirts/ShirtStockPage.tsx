import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Package, Plus } from 'lucide-react'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

// Hooks and components for Shirts
import { useShirtStock } from './hooks/useShirtStock'
import { useAdjustShirtStock } from './hooks/useAdjustShirtStock'
import { ShirtStockGrid } from './components/ShirtStockGrid'
import { ShirtStockMovementsTable } from './components/ShirtStockMovementsTable'
import { ManageProductsModal } from './components/ManageProductsModal'
import type { ShirtStockGridDto } from './types'

// Hooks and components for Generic Products
import { useGenericProductCategories } from '../generic/hooks/useGenericProductsApi'
import { GenericProductCategoryView } from '../generic/components/GenericProductCategoryView'
import { CreateCategoryModal } from '../generic/components/CreateCategoryModal'

// Hooks for SKU
import { useSkuCodes } from '@/features/separation/hooks/useSkuCodes'
import { CreateModelModal } from './components/CreateModelModal'

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
  const [activeTabId, setActiveTabId] = useState<string>('camisetas')
  const [modelCode, setModelCode] = useState<string>('REG')
  const [isEditMode, setIsEditMode] = useState(false)
  const [draftGrid, setDraftGrid] = useState<DraftGrid>({})
  const [isSaving, setIsSaving] = useState(false)
  const [isManageOpen, setIsManageOpen] = useState(false)
  const [isCreateCategoryOpen, setIsCreateCategoryOpen] = useState(false)
  const [isCreateModelOpen, setIsCreateModelOpen] = useState(false)

  const { data: genericCategories = [] } = useGenericProductCategories()
  const { data: skuCodes = [] } = useSkuCodes()
  const modelCodes = skuCodes.filter(c => c.category === 'Modelo')
  
  // If we have models from DB, ensure the current model is valid, else fallback
  const validModels = modelCodes.length > 0 ? modelCodes : [{ code: 'REG', value: 'Regular' }]
  const currentModelCode = validModels.find(m => m.code === modelCode)?.code || validModels[0].code

  const { data: shirtData, isLoading: isShirtLoading } = useShirtStock(currentModelCode)
  const adjustShirt = useAdjustShirtStock()
  const queryClient = useQueryClient()

  function handleTabChange(id: string) {
    setActiveTabId(id)
    setIsEditMode(false)
    setDraftGrid({})
  }

  function enterEditMode() {
    if (!shirtData) return
    setDraftGrid(buildDraftGrid(shirtData))
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
    if (!shirtData) return

    const payloads = []
    for (const row of shirtData.rows) {
      const draftRow = draftGrid[row.colorId] ?? {}
      for (const size of shirtData.sizes) {
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
          modelCode: currentModelCode,
        })
      }
    }

    if (payloads.length === 0) {
      cancelEditMode()
      return
    }

    setIsSaving(true)
    try {
      await Promise.all(payloads.map((p) => adjustShirt.mutateAsync(p)))
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock', currentModelCode] })
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock-movements', currentModelCode] })
    } finally {
      setIsSaving(false)
      setIsEditMode(false)
      setDraftGrid({})
    }
  }

  const activeCategory = genericCategories.find(c => c.id === activeTabId)

  return (
    <div className="space-y-6">
      <PageHeader
        title="Produtos"
        description={
          isEditMode
            ? 'Edite as quantidades de camisetas diretamente na tabela e confirme para salvar.'
            : 'Visão geral do estoque de produtos acabados.'
        }
        actions={
          activeTabId === 'camisetas' && !isEditMode ? (
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => setIsManageOpen(true)}
                className="gap-1.5"
              >
                <Package className="h-4 w-4" />
                Gerenciar camisetas
              </Button>
              <Button variant="outline" onClick={enterEditMode} disabled={!shirtData || isShirtLoading}>
                Ajuste manual
              </Button>
            </div>
          ) : activeTabId === 'camisetas' && isEditMode ? (
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

      {/* Abas Dinâmicas */}
      <div className="flex flex-wrap gap-0 border-b border-border items-center">
        <button
          onClick={() => handleTabChange('camisetas')}
          className={cn(
            'px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px',
            activeTabId === 'camisetas'
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground',
          )}
        >
          Camisetas
        </button>

        {genericCategories.map((cat) => (
          <button
            key={cat.id}
            onClick={() => handleTabChange(cat.id)}
            className={cn(
              'px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px',
              activeTabId === cat.id
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground',
            )}
          >
            {cat.name}
          </button>
        ))}

        <button
          onClick={() => setIsCreateCategoryOpen(true)}
          className="ml-2 flex items-center justify-center rounded-full bg-primary/10 text-primary hover:bg-primary/20 h-6 w-6 transition-colors"
          title="Adicionar nova categoria de produto"
        >
          <Plus className="h-4 w-4" />
        </button>
      </div>

      {activeTabId === 'camisetas' ? (
        <div className="space-y-4">
          {/* Sub-abas Modelos */}
          <div className="flex gap-1 items-center">
            {validModels.map((m) => (
              <button
                key={m.code}
                type="button"
                onClick={() => { setModelCode(m.code); setIsEditMode(false); setDraftGrid({}) }}
                className={cn(
                  'rounded-md px-3 py-1.5 text-xs font-medium transition-colors',
                  currentModelCode === m.code
                    ? 'bg-accent/10 text-accent'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                )}
                title={m.value}
              >
                {m.code} - {m.value}
              </button>
            ))}
            <button
              onClick={() => setIsCreateModelOpen(true)}
              className="ml-1 flex items-center justify-center rounded-full bg-accent/10 text-accent hover:bg-accent/20 h-5 w-5 transition-colors"
              title="Cadastrar novo modelo"
            >
              <Plus className="h-3 w-3" />
            </button>
          </div>

          {isShirtLoading ? (
            <Skeleton className="h-48 w-full rounded-lg" />
          ) : shirtData ? (
            <ShirtStockGrid
              data={shirtData}
              editMode={isEditMode}
              draftQuantities={isEditMode ? draftGrid : undefined}
              onQuantityChange={handleQuantityChange}
            />
          ) : null}

          {!isEditMode && (
            <div className="space-y-3">
              <h2 className="text-sm font-semibold text-foreground">Histórico de movimentações (Camisetas)</h2>
              <ShirtStockMovementsTable modelCode={currentModelCode} />
            </div>
          )}
        </div>
      ) : activeCategory ? (
        <GenericProductCategoryView
          key={activeCategory.id}
          categoryId={activeCategory.id}
          categoryName={activeCategory.name}
          onCategoryDeleted={() => setActiveTabId('camisetas')}
        />
      ) : (
        <div className="rounded-lg border border-border bg-card p-12 text-center text-sm text-muted-foreground">
          Categoria não encontrada.
        </div>
      )}

      {/* Modais */}
      <ManageProductsModal
        open={isManageOpen}
        onClose={() => {
          setIsManageOpen(false)
          queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
          queryClient.invalidateQueries({ queryKey: ['shirt-stock-movements'] })
        }}
        currentModelCode={currentModelCode}
      />

      <CreateCategoryModal
        open={isCreateCategoryOpen}
        onClose={() => setIsCreateCategoryOpen(false)}
      />

      <CreateModelModal
        open={isCreateModelOpen}
        onClose={() => setIsCreateModelOpen(false)}
      />
    </div>
  )
}
