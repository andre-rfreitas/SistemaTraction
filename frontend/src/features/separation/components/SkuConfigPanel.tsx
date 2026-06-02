import { useState } from 'react'
import { useSkuCodes, useUpsertSkuCode, useDeleteSkuCode } from '../hooks/useSkuCodes'
import { useDtfModels } from '../../settings/dtf/hooks/useDtfModels'
import type { SkuCodeCategory, SkuCodeDto } from '../types'
import type { DtfModelDto } from '../../settings/dtf/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

const CATEGORIES: { value: SkuCodeCategory; label: string }[] = [
  { value: 'Modelo',     label: 'Modelo' },
  { value: 'EstampaDtf', label: 'Estampa DTF' },
  { value: 'Cor',        label: 'Cor' },
  { value: 'Tamanho',    label: 'Tamanho' },
]

type CategoryBadgeVariant = 'info' | 'primary' | 'warning' | 'success'
const CATEGORY_VARIANT: Record<SkuCodeCategory, CategoryBadgeVariant> = {
  Modelo:     'primary',
  EstampaDtf: 'info',
  Cor:        'warning',
  Tamanho:    'success',
}

const EMPTY_FORM = {
  code: '', value: '', category: 'Cor' as SkuCodeCategory, dtfModelId: null as string | null,
}

export function SkuConfigPanel() {
  const { data: codes = [], isLoading } = useSkuCodes()
  const { data: dtfModels = [] } = useDtfModels()
  const upsert = useUpsertSkuCode()
  const del = useDeleteSkuCode()

  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [filterCategory, setFilterCategory] = useState<SkuCodeCategory | 'all'>('all')

  function handleEdit(c: SkuCodeDto) {
    setEditingId(c.id)
    setForm({ code: c.code, value: c.value, category: c.category, dtfModelId: c.dtfModelId })
  }

  function handleCancel() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function handleCategoryChange(cat: SkuCodeCategory) {
    setForm(f => ({ ...f, category: cat, dtfModelId: null, value: '' }))
  }

  function handleDtfModelChange(modelId: string) {
    const model = dtfModels.find((m: DtfModelDto) => m.id === modelId)
    setForm(f => ({ ...f, dtfModelId: modelId || null, value: model?.name ?? f.value }))
  }

  function handleSave() {
    if (!form.code.trim()) return
    if (form.category === 'EstampaDtf' && !form.dtfModelId) return
    if (form.category !== 'EstampaDtf' && !form.value.trim()) return

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
          <code className="bg-muted px-1 rounded">REG-REDR-RED-G</code>).
          Após configurar, o upload do PDF preencherá Cor, Tamanho e Estampa DTF automaticamente.
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
              placeholder="ex: RED"
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

          {/* Valor — DtfModel dropdown for EstampaDtf, text field otherwise */}
          {form.category === 'EstampaDtf' ? (
            <div>
              <label className="text-xs text-muted-foreground mb-0.5 block">Modelo DTF vinculado</label>
              <select
                value={form.dtfModelId ?? ''}
                onChange={(e) => handleDtfModelChange(e.target.value)}
                className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
              >
                <option value="">— Selecione —</option>
                {dtfModels.map((m: DtfModelDto) => <option key={m.id} value={m.id}>{m.name}</option>)}
              </select>
              {dtfModels.length === 0 && (
                <p className="text-xs text-warning mt-0.5">Nenhum modelo DTF cadastrado ainda.</p>
              )}
            </div>
          ) : (
            <div>
              <label className="text-xs text-muted-foreground mb-0.5 block">
                {form.category === 'Cor' ? 'Nome da cor (deve coincidir com o estoque)' :
                 form.category === 'Tamanho' ? 'Tamanho (ex: G, M, GG, G1)' : 'Valor'}
              </label>
              <Input
                value={form.value}
                onChange={(e) => setForm(f => ({ ...f, value: e.target.value }))}
                placeholder={
                  form.category === 'Cor' ? 'ex: Vermelho' :
                  form.category === 'Tamanho' ? 'ex: G' : 'ex: Regular'
                }
                className="text-sm"
              />
            </div>
          )}
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
            disabled={
              upsert.isPending ||
              !form.code.trim() ||
              (form.category === 'EstampaDtf' ? !form.dtfModelId : !form.value.trim())
            }
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
                <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Valor / Modelo</th>
                <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Categoria</th>
                <th className="px-3 py-2 text-right text-xs font-medium text-muted-foreground">Ações</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((c, i) => {
                const linkedModel = c.dtfModelId
                  ? dtfModels.find((m: DtfModelDto) => m.id === c.dtfModelId)
                  : null
                return (
                  <tr key={c.id} className={i % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                    <td className="px-3 py-2 font-mono font-semibold text-foreground">{c.code}</td>
                    <td className="px-3 py-2 text-foreground">
                      {linkedModel ? (
                        <span className="flex items-center gap-1">
                          <span className="text-info">◉</span> {linkedModel.name}
                        </span>
                      ) : c.value}
                    </td>
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
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* SKU preview */}
      {codes.length > 0 && <SkuPreview codes={codes} dtfModels={dtfModels} />}
    </div>
  )
}

function SkuPreview({ codes, dtfModels }: { codes: SkuCodeDto[]; dtfModels: DtfModelDto[] }) {
  const [sku, setSku] = useState('REG-REDR-RED-G')

  const byCode = new Map(codes.map(c => [c.code.toUpperCase(), c]))
  const tamanhoCodes = codes
    .filter(c => c.category === 'Tamanho')
    .map(c => c.code)
    .sort((a, b) => b.length - a.length)

  const parts = sku.toUpperCase().split('-').filter(Boolean)

  const resolved = parts.map(part => {
    const direct = byCode.get(part)
    if (direct) {
      const dtfModel = direct.dtfModelId ? dtfModels.find(m => m.id === direct.dtfModelId) : null
      return { part, code: direct, display: dtfModel ? dtfModel.name : direct.value }
    }
    for (const sizeCode of tamanhoCodes) {
      if (part.length > sizeCode.length && part.endsWith(sizeCode)) {
        const colorPart = part.slice(0, -sizeCode.length)
        const colorCode = byCode.get(colorPart)
        const sizeCodeObj = byCode.get(sizeCode)
        if (colorCode) return { part, splitColor: colorCode, splitSize: sizeCodeObj }
      }
    }
    return { part }
  })

  return (
    <div className="rounded-md bg-muted border border-border p-3 space-y-2">
      <p className="text-xs font-medium text-muted-foreground">Visualização de resolução de SKU</p>
      <div className="flex items-center gap-2 flex-wrap">
        <input
          value={sku}
          onChange={(e) => setSku(e.target.value)}
          className="font-mono text-xs border border-input rounded px-2 py-1 w-44 bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          placeholder="REG-REDR-RED-G"
        />
        <span className="text-xs text-muted-foreground">→</span>
        <div className="flex gap-1.5 flex-wrap">
          {resolved.map((r, i) => (
            <span key={i} className={`text-xs px-1.5 py-0.5 rounded font-medium ${
              r.code ? `${getBadgeClasses(CATEGORY_VARIANT[r.code.category])}` :
              r.splitColor ? 'bg-muted border border-border text-foreground' :
              'bg-muted text-muted-foreground'
            }`}>
              {r.code ? `${r.part} → ${r.display}` :
               r.splitColor ? `${r.part} → ${r.splitColor.value}${r.splitSize ? ` + ${r.splitSize.value}` : ''}` :
               `${r.part} (?)`}
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
    case 'info':    return 'bg-info/10 text-info border border-info/20'
    case 'warning': return 'bg-warning/10 text-warning border border-warning/20'
    case 'success': return 'bg-success/10 text-success border border-success/20'
  }
}
