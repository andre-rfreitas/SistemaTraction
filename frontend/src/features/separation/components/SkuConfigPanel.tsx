import { useState } from 'react'
import { useSkuCodes, useUpsertSkuCode, useDeleteSkuCode } from '../hooks/useSkuCodes'
import { useDtfModels } from '../../settings/dtf/hooks/useDtfModels'
import type { SkuCodeCategory, SkuCodeDto } from '../types'
import type { DtfModelDto } from '../../settings/dtf/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

const CATEGORIES: { value: SkuCodeCategory; label: string }[] = [
  { value: 'Modelo',     label: 'Modelo' },
  { value: 'EstampaDtf', label: 'Estampa DTF' },
  { value: 'Cor',        label: 'Cor' },
  { value: 'Tamanho',    label: 'Tamanho' },
]

const CATEGORY_COLOR: Record<SkuCodeCategory, string> = {
  Modelo:     'bg-purple-100 text-purple-800',
  EstampaDtf: 'bg-blue-100 text-blue-800',
  Cor:        'bg-orange-100 text-orange-800',
  Tamanho:    'bg-green-100 text-green-800',
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
        <h3 className="text-base font-semibold text-neutral-900">Configuração de Códigos SKU</h3>
        <p className="text-xs text-neutral-500 mt-0.5">
          Defina o significado de cada parte do SKU (ex:{' '}
          <code className="bg-neutral-100 px-1 rounded">REG-REDR-RED-G</code>).
          Após configurar, o upload do PDF preencherá Cor, Tamanho e Estampa DTF automaticamente.
        </p>
      </div>

      {/* Form */}
      <div className="border border-neutral-200 rounded-lg p-4 bg-neutral-50 space-y-3">
        <p className="text-xs font-medium text-neutral-600 uppercase tracking-wide">
          {editingId ? 'Editar código' : 'Adicionar código'}
        </p>

        <div className="grid grid-cols-3 gap-2">
          {/* Código */}
          <div>
            <label className="text-xs text-neutral-500 mb-0.5 block">Código (parte do SKU)</label>
            <Input
              value={form.code}
              onChange={(e) => setForm(f => ({ ...f, code: e.target.value.toUpperCase() }))}
              placeholder="ex: RED"
              className="font-mono text-sm uppercase"
            />
          </div>

          {/* Categoria */}
          <div>
            <label className="text-xs text-neutral-500 mb-0.5 block">Categoria</label>
            <select
              value={form.category}
              onChange={(e) => handleCategoryChange(e.target.value as SkuCodeCategory)}
              className="w-full border border-neutral-200 rounded-md px-2 py-2 text-sm bg-white"
            >
              {CATEGORIES.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
            </select>
          </div>

          {/* Valor — DtfModel dropdown for EstampaDtf, text field otherwise */}
          {form.category === 'EstampaDtf' ? (
            <div>
              <label className="text-xs text-neutral-500 mb-0.5 block">Modelo DTF vinculado</label>
              <select
                value={form.dtfModelId ?? ''}
                onChange={(e) => handleDtfModelChange(e.target.value)}
                className="w-full border border-neutral-200 rounded-md px-2 py-2 text-sm bg-white"
              >
                <option value="">— Selecione —</option>
                {dtfModels.map((m: DtfModelDto) => <option key={m.id} value={m.id}>{m.name}</option>)}
              </select>
              {dtfModels.length === 0 && (
                <p className="text-xs text-amber-600 mt-0.5">Nenhum modelo DTF cadastrado ainda.</p>
              )}
            </div>
          ) : (
            <div>
              <label className="text-xs text-neutral-500 mb-0.5 block">
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
          <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded px-2 py-1">
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
          <p className="text-xs text-red-600">
            {(upsert.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao salvar.'}
          </p>
        )}
      </div>

      {/* Filter tabs */}
      <div className="flex gap-1 border-b border-neutral-200">
        {(['all', ...CATEGORIES.map(c => c.value)] as const).map((cat) => (
          <button
            key={cat}
            onClick={() => setFilterCategory(cat as typeof filterCategory)}
            className={`px-3 py-1.5 text-xs font-medium border-b-2 -mb-px transition-colors ${
              filterCategory === cat
                ? 'border-neutral-900 text-neutral-900'
                : 'border-transparent text-neutral-500 hover:text-neutral-700'
            }`}
          >
            {cat === 'all' ? 'Todos' : CATEGORIES.find(c => c.value === cat)?.label}
            {cat !== 'all' && (
              <span className="ml-1 text-neutral-400">
                ({codes.filter(c => c.category === cat).length})
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Table */}
      {isLoading && <p className="text-sm text-neutral-500">Carregando...</p>}
      {!isLoading && filtered.length === 0 && (
        <p className="text-sm text-neutral-500">Nenhum código cadastrado.</p>
      )}

      {filtered.length > 0 && (
        <div className="border border-neutral-200 rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-neutral-50">
              <tr>
                <th className="px-3 py-2 text-left text-xs font-medium text-neutral-500">Código</th>
                <th className="px-3 py-2 text-left text-xs font-medium text-neutral-500">Valor / Modelo</th>
                <th className="px-3 py-2 text-left text-xs font-medium text-neutral-500">Categoria</th>
                <th className="px-3 py-2 text-right text-xs font-medium text-neutral-500">Ações</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((c, i) => {
                const linkedModel = c.dtfModelId
                  ? dtfModels.find((m: DtfModelDto) => m.id === c.dtfModelId)
                  : null
                return (
                  <tr key={c.id} className={i % 2 === 0 ? 'bg-white' : 'bg-neutral-50'}>
                    <td className="px-3 py-2 font-mono font-semibold text-neutral-900">{c.code}</td>
                    <td className="px-3 py-2 text-neutral-700">
                      {linkedModel ? (
                        <span className="flex items-center gap-1">
                          <span className="text-blue-600">◉</span> {linkedModel.name}
                        </span>
                      ) : c.value}
                    </td>
                    <td className="px-3 py-2">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${CATEGORY_COLOR[c.category]}`}>
                        {CATEGORIES.find(cat => cat.value === c.category)?.label ?? c.category}
                      </span>
                    </td>
                    <td className="px-3 py-2 text-right">
                      <div className="flex gap-1 justify-end">
                        <button onClick={() => handleEdit(c)}
                          className="text-xs text-blue-600 hover:text-blue-800 px-2 py-0.5 rounded hover:bg-blue-50">
                          Editar
                        </button>
                        <button onClick={() => del.mutate(c.id)} disabled={del.isPending}
                          className="text-xs text-red-500 hover:text-red-700 px-2 py-0.5 rounded hover:bg-red-50">
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
    <div className="rounded-md bg-neutral-50 border border-neutral-200 p-3 space-y-2">
      <p className="text-xs font-medium text-neutral-600">Visualização de resolução de SKU</p>
      <div className="flex items-center gap-2 flex-wrap">
        <input
          value={sku}
          onChange={(e) => setSku(e.target.value)}
          className="font-mono text-xs border border-neutral-300 rounded px-2 py-1 w-44"
          placeholder="REG-REDR-RED-G"
        />
        <span className="text-xs text-neutral-400">→</span>
        <div className="flex gap-1.5 flex-wrap">
          {resolved.map((r, i) => (
            <span key={i} className={`text-xs px-1.5 py-0.5 rounded font-medium ${
              r.code ? CATEGORY_COLOR[r.code.category] :
              r.splitColor ? 'bg-gradient-to-r from-orange-100 to-green-100 border border-neutral-200 text-neutral-700' :
              'bg-neutral-100 text-neutral-400'
            }`}>
              {r.code ? `${r.part} → ${r.display}` :
               r.splitColor ? `${r.part} → ${r.splitColor.value}${r.splitSize ? ` + ${r.splitSize.value}` : ''}` :
               `${r.part} (?)`}
            </span>
          ))}
        </div>
      </div>
      <p className="text-xs text-neutral-400">(?) = código não configurado</p>
    </div>
  )
}
