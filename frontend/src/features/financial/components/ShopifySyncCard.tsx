import { useState } from 'react'
import { RefreshCw } from 'lucide-react'
import type { FinancialEntryDto } from '../types'
import { useShopifySync } from '../hooks/useShopifySync'
import { formatBRL } from '../format'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'

interface Props {
  entries: FinancialEntryDto[]
  periodFrom: string
  periodTo: string
}

function toDateInput(iso: string): string {
  return iso.slice(0, 10)
}

function currentMonthRange() {
  const now = new Date()
  const from = new Date(now.getFullYear(), now.getMonth(), 1)
    .toISOString()
    .slice(0, 10)
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0)
    .toISOString()
    .slice(0, 10)
  return { from, to }
}

export function ShopifySyncCard({ entries, periodFrom, periodTo }: Props) {
  const [open, setOpen] = useState(false)
  const defaults = currentMonthRange()
  const [syncFrom, setSyncFrom] = useState(
    toDateInput(periodFrom) || defaults.from
  )
  const [syncTo, setSyncTo] = useState(toDateInput(periodTo) || defaults.to)
  const sync = useShopifySync()

  const shopifyEntries = entries.filter(
    (e) => e.referenceType === 'ShopifyOrder' && !e.isReversal
  )

  const totalAmount = shopifyEntries.reduce((sum, e) => sum + e.amount, 0)

  const bySource = shopifyEntries.reduce<Record<string, number>>((acc, e) => {
    acc[e.category] = (acc[e.category] ?? 0) + e.amount
    return acc
  }, {})

  function handleSync() {
    sync.mutate(
      { from: syncFrom, to: syncTo },
      { onSuccess: () => setOpen(false) }
    )
  }

  return (
    <>
      <div className="rounded-xl border border-border bg-card p-4 space-y-3">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <span className="flex h-7 w-7 items-center justify-center rounded-md bg-[#96BF48]/10">
              <ShopifyIcon />
            </span>
            <span className="text-sm font-semibold text-foreground">Receitas Shopify</span>
          </div>
          <Button
            size="sm"
            variant="outline"
            onClick={() => {
              setSyncFrom(toDateInput(periodFrom) || defaults.from)
              setSyncTo(toDateInput(periodTo) || defaults.to)
              setOpen(true)
            }}
          >
            <RefreshCw className="h-3.5 w-3.5 mr-1.5" />
            Sincronizar período
          </Button>
        </div>

        <div>
          <p className="text-2xl font-bold tabular-nums text-foreground">
            {formatBRL(totalAmount)}
          </p>
          {shopifyEntries.length === 0 ? (
            <p className="text-xs text-muted-foreground mt-1">
              Nenhuma receita Shopify neste período
            </p>
          ) : (
            <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2">
              {Object.entries(bySource).map(([cat, val]) => (
                <span key={cat} className="text-xs text-muted-foreground">
                  {cat}{' '}
                  <span className="font-medium text-foreground tabular-nums">
                    {formatBRL(val)}
                  </span>
                </span>
              ))}
            </div>
          )}
        </div>
      </div>

      <Dialog open={open} onOpenChange={(v) => !sync.isPending && setOpen(v)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Sincronizar pedidos Shopify</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label htmlFor="sync-from" className="text-xs">De</Label>
                <Input
                  id="sync-from"
                  type="date"
                  value={syncFrom}
                  onChange={(e) => setSyncFrom(e.target.value)}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="sync-to" className="text-xs">Até</Label>
                <Input
                  id="sync-to"
                  type="date"
                  value={syncTo}
                  onChange={(e) => setSyncTo(e.target.value)}
                />
              </div>
            </div>

            {syncFrom && syncTo && (
              <p className="text-sm text-muted-foreground">
                Sincronizar pedidos pagos de{' '}
                <span className="font-medium text-foreground">{syncFrom}</span> até{' '}
                <span className="font-medium text-foreground">{syncTo}</span>
              </p>
            )}

            {sync.isSuccess && sync.data && (
              <div className="rounded-md border border-border bg-muted/40 p-3 space-y-1 text-sm">
                <p className="font-medium text-foreground">
                  {sync.data.totalImported} pedido(s) importado(s) —{' '}
                  {formatBRL(sync.data.totalAmount)} em receitas
                </p>
                {sync.data.totalSkipped > 0 && (
                  <p className="text-muted-foreground">
                    {sync.data.totalSkipped} pedido(s) já existentes ignorados
                  </p>
                )}
                {sync.data.errors.length > 0 && (
                  <ul className="mt-2 space-y-0.5">
                    {sync.data.errors.map((err, i) => (
                      <li key={i} className="text-xs text-amber-600">
                        {err}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            )}

            {sync.isError && (
              <p className="text-sm text-danger">{(sync.error as Error).message}</p>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)} disabled={sync.isPending}>
              Cancelar
            </Button>
            <Button
              onClick={handleSync}
              isLoading={sync.isPending}
              disabled={!syncFrom || !syncTo}
            >
              Sincronizar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

function ShopifyIcon() {
  return (
    <svg
      viewBox="0 0 109.5 124.5"
      className="h-4 w-4"
      fill="#96BF48"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path d="M74.7 14.8s-.3 0-.6.1c-.3-1-1-1.8-1.8-1.8-.7 0-1.4.5-2 1.3-1.9-.4-3.3.8-3.9 2.5-.1 0-.3 0-.5 0-1.1 0-2.4 1.6-2.9 4.3l12.5 3.9c.1-4.9-1.2-9.5-1.8-10.3zm-21.4 5.3l-1.1 3.4L26.1 117l41.6 7.5 41.6-7.5L83.2 23.5l-29.9-3.4zm-8 23.5l1.1-3.4 25.1 7.3-1.1 3.4-25.1-7.3zm2.1-6.6l1.1-3.4 25.1 7.3-1.1 3.4-25.1-7.3zm4.2-13l1.1-3.4 25.1 7.3-1.1 3.4L51.6 24z" />
    </svg>
  )
}
