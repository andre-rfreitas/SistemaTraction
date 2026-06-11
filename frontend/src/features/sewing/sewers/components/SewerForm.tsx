import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { SewerDto, CreateSewerInput, ProductTypeInput } from '../types'

interface Props {
  defaultValues?: SewerDto
  isLoading: boolean
  onSubmit: (data: CreateSewerInput) => void
  onCancel: () => void
}

interface ProductTypeRow {
  key: number
  name: string
  priceDefault: string
  priceG1: string
}

let rowKey = 0

function makeRow(pt?: ProductTypeInput): ProductTypeRow {
  return {
    key: rowKey++,
    name: pt?.name ?? '',
    priceDefault: pt?.priceDefault != null ? String(pt.priceDefault) : '',
    priceG1: pt?.priceG1 != null ? String(pt.priceG1) : '',
  }
}

export function SewerForm({ defaultValues, isLoading, onSubmit, onCancel }: Props) {
  const [name, setName] = useState(defaultValues?.name ?? '')
  const [phone, setPhone] = useState(defaultValues?.phone ?? '')
  const [rows, setRows] = useState<ProductTypeRow[]>(() =>
    defaultValues && defaultValues.productTypes.length > 0
      ? defaultValues.productTypes.map((pt) => makeRow(pt))
      : [makeRow()]
  )
  const [error, setError] = useState<string | null>(null)

  function updateRow(key: number, field: keyof Omit<ProductTypeRow, 'key'>, value: string) {
    setRows((prev) =>
      prev.map((r) => (r.key === key ? { ...r, [field]: value } : r))
    )
  }

  function addRow() {
    setRows((prev) => [...prev, makeRow()])
  }

  function removeRow(key: number) {
    setRows((prev) => prev.filter((r) => r.key !== key))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)

    if (!name.trim()) {
      setError('Nome é obrigatório.')
      return
    }

    const productTypes: ProductTypeInput[] = []
    for (const row of rows) {
      if (!row.name.trim()) continue
      const pd = parseFloat(row.priceDefault.replace(',', '.'))
      const pg1 = parseFloat(row.priceG1.replace(',', '.'))
      if (isNaN(pd) || pd <= 0 || isNaN(pg1) || pg1 <= 0) {
        setError(`Preços inválidos para "${row.name}". Use valores maiores que zero.`)
        return
      }
      productTypes.push({ name: row.name.trim(), priceDefault: pd, priceG1: pg1 })
    }

    onSubmit({ name: name.trim(), phone: phone.trim() || undefined, productTypes })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div className="space-y-3">
        <div>
          <label className="text-sm font-medium text-foreground block mb-1">Nome *</label>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Nome da costureira"
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-foreground block mb-1">Telefone</label>
          <input
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            placeholder="(11) 99999-9999"
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          />
        </div>
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <label className="text-sm font-medium text-foreground">Tipos de produto</label>
          <Button type="button" variant="outline" size="sm" onClick={addRow}>
            <Plus className="size-3 mr-1" /> Adicionar
          </Button>
        </div>

        {rows.length === 0 ? (
          <p className="text-xs text-muted-foreground italic">Nenhum tipo adicionado.</p>
        ) : (
          <div className="space-y-2">
            <div className="grid grid-cols-[1fr_100px_100px_32px] gap-2 text-xs font-medium text-muted-foreground px-1">
              <span>Produto</span>
              <span className="text-center">P/M/G/GG</span>
              <span className="text-center">G1</span>
              <span />
            </div>
            {rows.map((row) => (
              <div key={row.key} className="grid grid-cols-[1fr_100px_100px_32px] gap-2 items-center">
                <input
                  value={row.name}
                  onChange={(e) => updateRow(row.key, 'name', e.target.value)}
                  placeholder="ex: Camiseta básica"
                  className="rounded-md border border-input bg-background px-2 py-1.5 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                />
                <input
                  value={row.priceDefault}
                  onChange={(e) => updateRow(row.key, 'priceDefault', e.target.value)}
                  placeholder="5,60"
                  className="rounded-md border border-input bg-background px-2 py-1.5 text-sm shadow-sm text-center focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                />
                <input
                  value={row.priceG1}
                  onChange={(e) => updateRow(row.key, 'priceG1', e.target.value)}
                  placeholder="6,30"
                  className="rounded-md border border-input bg-background px-2 py-1.5 text-sm shadow-sm text-center focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                />
                <Button
                  type="button"
                  size="icon"
                  variant="ghost"
                  className="text-destructive hover:text-destructive size-8"
                  onClick={() => removeRow(row.key)}
                >
                  <Trash2 className="size-3.5" />
                </Button>
              </div>
            ))}
          </div>
        )}
      </div>

      {error && (
        <p className="text-sm text-destructive">{error}</p>
      )}

      <div className="flex gap-2">
        <Button type="button" variant="outline" onClick={onCancel} className="flex-1" disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" className="flex-1" disabled={isLoading}>
          {isLoading ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
    </form>
  )
}
