import { useState } from 'react'
import { Button } from '@/components/ui/button'
import type { SupplyStockItemDto, SupplyStockMovementDto } from '../types'

interface Props {
  item: SupplyStockItemDto
  editing?: SupplyStockMovementDto
  isLoading: boolean
  onSubmit: (data: EntryFormData) => void
  onCancel: () => void
}

export interface EntryFormData {
  quantity: number
  supplierName: string
  supplierPhone: string
  occurredAt: string
  unitPrice: number | null
  totalCost: number | null
}

function todayLocal() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function toLocalDate(isoStr: string) {
  const d = new Date(isoStr)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

export function SupplyEntryForm({ item, editing, isLoading, onSubmit, onCancel }: Props) {
  const isEditMode = !!editing

  const [quantity, setQuantity] = useState(editing ? Math.abs(editing.delta) : 1)
  const [supplierName, setSupplierName] = useState(editing?.supplierName ?? '')
  const [supplierPhone, setSupplierPhone] = useState(editing?.supplierPhone ?? '')
  const [occurredAt, setOccurredAt] = useState(
    editing?.occurredAt ? toLocalDate(editing.occurredAt) : todayLocal()
  )
  const [unitPrice, setUnitPrice] = useState<string>(
    editing?.unitPrice != null
      ? String(editing.unitPrice)
      : item.pricePerUnit != null
      ? String(item.pricePerUnit)
      : ''
  )
  const [error, setError] = useState<string | null>(null)

  const parsedUnitPrice = parseFloat(unitPrice.replace(',', '.')) || null
  const totalCost = parsedUnitPrice != null && quantity > 0 ? parsedUnitPrice * quantity : null

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)

    if (!isEditMode && quantity <= 0) {
      setError('Quantidade deve ser maior que zero.')
      return
    }

    onSubmit({
      quantity,
      supplierName: supplierName.trim(),
      supplierPhone: supplierPhone.trim(),
      occurredAt: occurredAt ? `${occurredAt}T12:00:00Z` : new Date().toISOString(),
      unitPrice: parsedUnitPrice,
      totalCost,
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {!isEditMode && (
        <div className="space-y-1">
          <label className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
            Quantidade ({item.unit})
          </label>
          <input
            type="number"
            min="1"
            value={quantity}
            onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 1))}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
      )}

      {isEditMode && (
        <div className="rounded-md bg-muted/50 px-3 py-2 text-sm text-muted-foreground">
          Quantidade: <span className="font-semibold text-foreground">{Math.abs(editing!.delta)} {item.unit}</span>
          <span className="ml-1 text-xs">(não editável)</span>
        </div>
      )}

      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1">
          <label className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
            Fornecedor
          </label>
          <input
            value={supplierName}
            onChange={(e) => setSupplierName(e.target.value)}
            placeholder="Nome do fornecedor"
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
        <div className="space-y-1">
          <label className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
            WhatsApp
          </label>
          <input
            value={supplierPhone}
            onChange={(e) => setSupplierPhone(e.target.value)}
            placeholder="(11) 99999-9999"
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
      </div>

      <div className="space-y-1">
        <label className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
          Data da compra
        </label>
        <input
          type="date"
          value={occurredAt}
          onChange={(e) => setOccurredAt(e.target.value)}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      <div className="space-y-1">
        <label className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
          Preço por unidade (R$)
        </label>
        <input
          type="number"
          step="0.01"
          min="0"
          value={unitPrice}
          onChange={(e) => setUnitPrice(e.target.value)}
          placeholder={item.pricePerUnit != null ? `Padrão: R$ ${fmt(item.pricePerUnit)}` : 'Ex: 0,85'}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      {totalCost != null && (
        <div className="rounded-md bg-primary/10 border border-primary/20 px-4 py-3 flex items-center justify-between">
          <span className="text-sm text-muted-foreground">
            {quantity} {item.unit} × R$ {fmt(parsedUnitPrice!)}
          </span>
          <span className="text-lg font-bold text-primary">
            R$ {fmt(totalCost)}
          </span>
        </div>
      )}

      {error && <p className="text-sm text-danger">{error}</p>}

      <div className="flex gap-2">
        <Button type="button" variant="outline" onClick={onCancel} className="flex-1" disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" className="flex-1" disabled={isLoading}>
          {isLoading ? 'Registrando...' : isEditMode ? 'Salvar alterações' : 'Registrar entrada'}
        </Button>
      </div>
    </form>
  )
}
