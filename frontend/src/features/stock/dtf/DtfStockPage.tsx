import { useState } from 'react'
import { DtfStockList } from './components/DtfStockList'
import { DtfStockDetail } from './components/DtfStockDetail'
import { RegisterMovementForm } from './components/RegisterMovementForm'
import { useRegisterDtfMovement } from './hooks/useRegisterDtfMovement'
import type { DtfStockItemDto, DtfMovementType } from './types'
import { PageHeader } from '@/components/ui/page-header'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

type SelectedItem =
  | DtfStockItemDto
  | { dtfModelId: string; modelName: string; sheetLabel: string }

export function DtfStockPage() {
  const [selected, setSelected] = useState<SelectedItem | null>(null)
  const register = useRegisterDtfMovement()

  function handleClose() {
    setSelected(null)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Estoque de DTF"
        description="Posição atual de folhas por modelo. Registre entradas, saídas e ajustes."
      />

      <DtfStockList onSelect={setSelected} />

      <Dialog open={!!selected} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {selected?.modelName}{' '}
              <span className="text-muted-foreground font-normal text-base">
                — {selected?.sheetLabel}
              </span>
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-5">
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                Registrar movimento
              </p>
              <RegisterMovementForm
                isLoading={register.isPending}
                onSubmit={(data) => {
                  if (!selected) return
                  register.mutate(
                    {
                      dtfModelId: selected.dtfModelId,
                      type: data.type as DtfMovementType,
                      quantity: data.quantity,
                      reason: data.reason || null,
                    },
                    { onSuccess: handleClose }
                  )
                }}
              />
              {register.isError && (
                <p className="text-sm text-danger mt-2">
                  {(register.error as Error).message}
                </p>
              )}
            </div>

            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                Últimas movimentações
              </p>
              <DtfStockDetail dtfModelId={selected?.dtfModelId ?? ''} />
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
