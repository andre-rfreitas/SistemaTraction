import { useState } from 'react'
import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { useShirtStock } from './hooks/useShirtStock'
import { useAdjustShirtStock } from './hooks/useAdjustShirtStock'
import { ShirtStockGrid } from './components/ShirtStockGrid'
import { StockAdjustmentModal } from './components/StockAdjustmentModal'
import { ShirtStockMovementsTable } from './components/ShirtStockMovementsTable'

export function ShirtStockPage() {
  const [modalOpen, setModalOpen] = useState(false)
  const { data, isLoading } = useShirtStock()
  const adjust = useAdjustShirtStock()

  function handleClose() {
    setModalOpen(false)
  }

  function handleCellSave(colorId: string, _colorName: string, size: string, newQty: number) {
    const row = data?.rows.find((r) => r.colorId === colorId)
    const currentQty = row?.quantities[size] ?? 0
    const delta = newQty - currentQty
    if (delta === 0) return

    adjust.mutate({
      fabricColorId: colorId,
      size,
      adjustmentType: delta > 0 ? 'Entrada' : 'Saída',
      quantity: Math.abs(delta),
      reason: 'Ajuste manual',
    })
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Estoque de Camisetas"
        description="Visão geral do estoque por cor e tamanho. Ajustes manuais ficam registrados no histórico."
        actions={
          <Button variant="outline" onClick={() => setModalOpen(true)}>Ajuste manual</Button>
        }
      />

      {isLoading ? (
        <Skeleton className="h-48 w-full rounded-lg" />
      ) : data ? (
        <ShirtStockGrid
          data={data}
          isSaving={adjust.isPending}
          onCellSave={handleCellSave}
        />
      ) : null}

      <div className="space-y-3">
        <h2 className="text-sm font-semibold text-foreground">Histórico de movimentações</h2>
        <ShirtStockMovementsTable />
      </div>

      <Dialog open={modalOpen} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Ajuste manual de estoque</DialogTitle>
          </DialogHeader>
          <StockAdjustmentModal
            sizes={data?.sizes ?? ['P', 'M', 'G', 'G1', 'GG']}
            isLoading={adjust.isPending}
            onConfirm={(payload) =>
              adjust.mutate(payload, { onSuccess: handleClose })
            }
            onClose={handleClose}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
