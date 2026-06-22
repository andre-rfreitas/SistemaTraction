import { useState } from 'react'
import { useSkuCodes, useUpsertSkuCode, useDeleteSkuCode } from '../hooks/useSkuCodes'
import type { SkuCodeCategory, SkuCodeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

// Extensible: add new categories here as needed in the future
const CATEGORIES: { value: SkuCodeCategory; label: string }[] = [
  { value: 'Modelo',  label: 'Modelo' },
  { value: 'Cor',     label: 'Cor' },
  { value: 'Tamanho', label: 'Tamanho' },
]

type CategoryBadgeVariant = 'primary' | 'warning' | 'success'
const CATEGORY_VARIANT: Record<SkuCodeCategory, CategoryBadgeVariant> = {
  Modelo:  'primary',
  Cor:     'warning',
  Tamanho: 'success',
}

const EMPTY_FORM = {
  code: '', value: '', category: 'Cor' as SkuCodeCategory,
}

export function SkuConfigPanel() {
  const { data: codes = [], isLoading } = useSkuCodes()
  const upsert = useUpsertSkuCode()
  const del = useDeleteSkuCode()

  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [filterCategory, setFilterCategory] = useState<SkuCodeCategory | 'all'>('all')

  function handleEdit(c: SkuCodeDto) {
    setEditingId(c.id)
    setForm({ code: c.code, value: c.value, category: c.category })
  }

  function handleCancel() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function handleCategoryChange(cat: SkuCodeCategory) {
    setForm(f => ({ ...f, category: cat, value: '' }))
  }

  function handleSave() {
    if (!form.code.trim() || !form.value.trim()) return

    upsert.mutate(
      { id: editingId ?? undefined, ...form },
      { onSuccess: () => { setEditingId(null); setForm(EMPTY_FORM) } },
    )
  }

  const filtered = filterCategory === 'all' ? codes : codes.filter(c => c.category === filterCategory)

  return (
    <div className="space-y-5">
      <div>
        <h3 className="text-base font-semibold text-foreground">Configuração de Códigos SKU</h3>
        <p className="text-xs text-muted-foreground mt-0.5">
          Defina o significado de cada parte do SKU (ex:{' '}
          <code className="bg-muted px-1 rounded">BBL-BLK-M</code>).
          Formato: <strong>MODELO-COR-TAMANHO</strong>. Após configurar, o upload do PDF preencherá Cor e Tamanho automaticamente.
        </p>
      </div>

      {/* Form */}
      <div className="border border-border rounded-lg p-4 bg-muted space-y-3">
        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
          {editingId ? 'Editar código' : 'Adicionar código'}
        </p>

        <div className="grid grid-cols-3 gap-2">
          {/* Código */}
          <div>
            <label className="text-xs text-muted-foreground mb-0.5 block">Código (parte do SKU)</label>
            <Input
              value={form.code}
              onChange={(e) => setForm(f => ({ ...f, code: e.target.value.toUpperCase() }))}
              placeholder="ex: BLK"
              className="font-mono text-sm uppercase"
            />
          </div>

          {/* Categoria */}
          <div>
            <label className="text-xs text-muted-foreground mb-0.5 block">Categoria</label>
            <select
              value={form.category}
              onChange={(e) => handleCategoryChange(e.target.value as SkuCodeCategory)}
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
            >
              {CATEGORIES.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
            </select>
          </div>

          {/* Valor */}
          <div>
            <label className="text-xs text-muted-foreground mb-0.5 block">
              {form.category === 'Cor'     ? 'Nome da cor (deve coincidir com o estoque)' :
               form.category === 'Tamanho' ? 'Tamanho (ex: G, M, GG, G1)' : 'Valor (nome do modelo)'}
            </label>
            <Input
              value={form.value}
              onChange={(e) => setForm(f => ({ ...f, value: e.target.value }))}
              placeholder={
                form.category === 'Cor'     ? 'ex: Preto' :
                form.category === 'Tamanho' ? 'ex: G' : 'ex: Babylook'
              }
              className="text-sm"
            />
          </div>
        </div>

        {form.category === 'Cor' && (
          <p className="text-xs text-warning bg-warning/10 border border-warning/20 rounded px-2 py-1">
            O valor da Cor deve ser idêntico ao nome da cor cadastrada no estoque de camisetas.
          </p>
        )}

        <div className="flex gap-2">
          {editingId && (
            <Button variant="outline" onClick={handleCancel} className="text-sm h-8 px-3">Cancelar</Button>
          )}
          <Button
            onClick={handleSave}
            disabled={upsert.isPending || !form.code.trim() || !form.value.trim()}
            className="text-sm h-8 px-4"
          >
            {upsert.isPending ? 'Salvando...' : editingId ? 'Salvar alterações' : '+ Adicionar'}
          </Button>
        </div>
        {upsert.isError && (
          <p className="text-xs text-danger">
            {(upsert.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao salvar.'}
          </p>
        )}
      </div>

      {/* Filter tabs */}
      <div className="flex gap-1 border-b border-border">
        {(['all', ...CATEGORIES.map(c => c.value)] as const).map((cat) => (
          <button
            key={cat}
            onClick={() => setFilterCategory(cat as typeof filterCategory)}
            className={`px-3 py-1.5 text-xs font-medium border-b-2 -mb-px transition-colors ${
              filterCategory === cat
                ? 'border-foreground text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            {cat === 'all' ? 'Todos' : CATEGORIES.find(c => c.value === cat)?.label}
            {cat !== 'all' && (
              <span className="ml-1 text-muted-foreground">
                ({codes.filter(c => c.category === cat).length})
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Table */}
      {isLoading && <p className="text-sm text-muted-foreground">Carregando...</p>}
      {!isLoading && filtered.length === 0 && (
        <p className="text-sm text-muted-foreground">Nenhum código cadastrado.</p>
      )}

      {filtered.length > 0 && (
        <div className="border border-border rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted">
              <tr>
                <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Código</th>
                <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Valor</th>
                <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Categoria</th>
                <th className="px-3 py-2 text-right text-xs font-medium text-muted-foreground">Ações</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((c, i) => (
                <tr key={c.id} className={i % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                  <td className="px-3 py-2 font-mono font-semibold text-foreground">{c.code}</td>
                  <td className="px-3 py-2 text-foreground">{c.value}</td>
                  <td className="px-3 py-2">
                    <Badge variant={CATEGORY_VARIANT[c.category]}>
                      {CATEGORIES.find(cat => cat.value === c.category)?.label ?? c.category}
                    </Badge>
                  </td>
                  <td className="px-3 py-2 text-right">
                    <div className="flex gap-1 justify-end">
                      <button onClick={() => handleEdit(c)}
                        className="text-xs text-info hover:text-info/80 px-2 py-0.5 rounded hover:bg-info/10">
                        Editar
                      </button>
                      <button onClick={() => del.mutate(c.id)} disabled={del.isPending}
                        className="text-xs text-danger hover:text-danger/80 px-2 py-0.5 rounded hover:bg-danger/10">
                        Remover
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* SKU preview */}
      {codes.length > 0 && <SkuPreview codes={codes} />}
    </div>
  )
}

/**
 * Interactive preview that shows how a SKU in MODELO-COR-TAMANHO format
 * resolves against the configured codes.
 */
function SkuPreview({ codes }: { codes: SkuCodeDto[] }) {
  const [sku, setSku] = useState('BBL-BLK-M')

  const byCode = new Map(codes.map(c => [c.code.toUpperCase(), c]))

  // SKU format: MODELO-COR-TAMANHO (3 segments)
  const parts = sku.toUpperCase().split('-').filter(Boolean)

  const resolved = parts.map((part, index) => {
    const match = byCode.get(part)
    // Infer expected category by position
    const expectedCategory: SkuCodeCategory | null =
      index === 0 ? 'Modelo' : index === 1 ? 'Cor' : index === 2 ? 'Tamanho' : null

    if (match) {
      return { part, code: match, display: match.value }
    }
    return { part, code: null, expectedCategory, display: null }
  })

  return (
    <div className="rounded-md bg-muted border border-border p-3 space-y-2">
      <p className="text-xs font-medium text-muted-foreground">Visualização de resolução de SKU</p>
      <div className="flex items-center gap-2 flex-wrap">
        <input
          value={sku}
          onChange={(e) => setSku(e.target.value)}
          className="font-mono text-xs border border-input rounded px-2 py-1 w-44 bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          placeholder="BBL-BLK-M"
        />
        <span className="text-xs text-muted-foreground">→</span>
        <div className="flex gap-1.5 flex-wrap">
          {resolved.map((r, i) => (
            <span key={i} className={`text-xs px-1.5 py-0.5 rounded font-medium ${
              r.code ? getBadgeClasses(CATEGORY_VARIANT[r.code.category]) :
              'bg-muted text-muted-foreground border border-border'
            }`}>
              {r.code
                ? `${r.part} → ${r.display}`
                : `${r.part} (?)`}
            </span>
          ))}
        </div>
      </div>
      <p className="text-xs text-muted-foreground">(?) = código não configurado</p>
    </div>
  )
}

function getBadgeClasses(variant: CategoryBadgeVariant): string {
  switch (variant) {
    case 'primary': return 'bg-primary/10 text-primary border border-primary/20'
    case 'warning': return 'bg-warning/10 text-warning border border-warning/20'
    case 'success': return 'bg-success/10 text-success border border-success/20'
  }
}
