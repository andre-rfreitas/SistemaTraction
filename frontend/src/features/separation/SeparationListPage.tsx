import { useState, useRef, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useSeparationLists } from './hooks/useSeparationLists'
import { useUploadSeparationList } from './hooks/useUploadSeparationList'
import { useUpdateSeparationItems } from './hooks/useUpdateSeparationItems'
import { useStockCheck } from './hooks/useStockCheck'
import { useConfirmSeparationList } from './hooks/useConfirmSeparationList'
import type {
  SeparationListDetail,
  SeparationItemDto,
  StockCheckResult,
  SeparationConfirmResult,
} from './types'
import { Button } from '@/components/ui/button'
import { useDtfModels } from '../settings/dtf/hooks/useDtfModels'
import type { DtfModelDto } from '../settings/dtf/types'

type Step = 'list' | 'upload' | 'review' | 'stock-check' | 'confirm-modal' | 'done'

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Pendente',
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
}
const STATUS_COLOR: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-800',
  Confirmed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
}

const fmt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2 })

export function SeparationListPage() {
  const [step, setStep] = useState<Step>('list')
  const [currentList, setCurrentList] = useState<SeparationListDetail | null>(null)
  const [editedItems, setEditedItems] = useState<SeparationItemDto[]>([])
  const [stockCheck, setStockCheck] = useState<StockCheckResult | null>(null)
  const [confirmResult, setConfirmResult] = useState<SeparationConfirmResult | null>(null)
  const [pdfBlobUrl, setPdfBlobUrl] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { data: lists = [], isLoading } = useSeparationLists()
  const { data: dtfModels = [] } = useDtfModels()
  const upload = useUploadSeparationList()
  const updateItems = useUpdateSeparationItems()
  const stockCheckQuery = useStockCheck(step === 'stock-check' ? currentList?.id ?? null : null)
  const confirm = useConfirmSeparationList()

  function handleFileSelect(file: File) {
    const url = URL.createObjectURL(file)
    setPdfBlobUrl(url)
    upload.mutate(file, {
      onSuccess: (list) => {
        setCurrentList(list)
        setEditedItems([...list.items])
        setStep('review')
      },
    })
  }

  function handleSaveAndCheck() {
    if (!currentList) return
    updateItems.mutate(
      { listId: currentList.id, items: editedItems },
      {
        onSuccess: (updated) => {
          setCurrentList(updated)
          setEditedItems([...updated.items])
          setStep('stock-check')
        },
      },
    )
  }

  function handleStockCheckLoaded(result: StockCheckResult) {
    setStockCheck(result)
  }

  function handleConfirm() {
    if (!currentList) return
    confirm.mutate(currentList.id, {
      onSuccess: (result) => {
        setConfirmResult(result)
        setStep('done')
      },
    })
  }

  async function handleCopy(text: string) {
    await navigator.clipboard.writeText(text)
    setCopied(true)
    setTimeout(() => setCopied(false), 2500)
  }

  function handleReset() {
    if (pdfBlobUrl) URL.revokeObjectURL(pdfBlobUrl)
    setPdfBlobUrl(null)
    setCurrentList(null)
    setEditedItems([])
    setStockCheck(null)
    setConfirmResult(null)
    setStep('list')
  }

  // ── LIST VIEW ──────────────────────────────────────────────────────────────
  if (step === 'list') {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h2 className="text-xl font-bold text-neutral-900">Lista de Separação</h2>
            <p className="text-sm text-neutral-500">Importar PDF do ERP e processar pedidos.</p>
          </div>
          <Button onClick={() => setStep('upload')}>+ Nova lista</Button>
        </div>

        {isLoading && <p className="text-sm text-neutral-500">Carregando...</p>}
        {!isLoading && lists.length === 0 && (
          <p className="text-sm text-neutral-500">Nenhuma lista importada ainda.</p>
        )}

        <div className="space-y-2">
          {lists.map((l) => (
            <div key={l.id} className="border border-neutral-200 rounded-lg p-4 bg-white flex justify-between items-center">
              <div>
                <p className="text-sm font-semibold text-neutral-900">{l.fileName}</p>
                <p className="text-xs text-neutral-500 mt-0.5">
                  {new Date(l.uploadedAt).toLocaleDateString('pt-BR')} —{' '}
                  {l.itemCount} itens — {l.totalQuantity} peças
                </p>
              </div>
              <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLOR[l.status] ?? 'bg-neutral-100'}`}>
                {STATUS_LABEL[l.status] ?? l.status}
              </span>
            </div>
          ))}
        </div>
      </div>
    )
  }

  // ── UPLOAD STEP ────────────────────────────────────────────────────────────
  if (step === 'upload') {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-3">
          <button onClick={handleReset} className="text-sm text-neutral-500 hover:text-neutral-700">← Voltar</button>
          <h2 className="text-xl font-bold text-neutral-900">Nova lista de separação</h2>
        </div>

        <div
          className="border-2 border-dashed border-neutral-300 rounded-xl p-12 text-center cursor-pointer hover:border-neutral-400 transition-colors"
          onClick={() => fileInputRef.current?.click()}
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => {
            e.preventDefault()
            const file = e.dataTransfer.files[0]
            if (file?.type === 'application/pdf') handleFileSelect(file)
          }}
        >
          <div className="text-4xl mb-3">📄</div>
          <p className="text-sm font-medium text-neutral-700">Clique ou arraste o PDF aqui</p>
          <p className="text-xs text-neutral-400 mt-1">Apenas arquivos .pdf do ERP</p>
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf"
            className="hidden"
            onChange={(e) => {
              const file = e.target.files?.[0]
              if (file) handleFileSelect(file)
            }}
          />
        </div>

        {upload.isPending && (
          <p className="text-sm text-neutral-500 text-center">Processando PDF...</p>
        )}
        {upload.isError && (
          <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">
            {(upload.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao processar o PDF.'}
          </div>
        )}
      </div>
    )
  }

  // ── REVIEW STEP ────────────────────────────────────────────────────────────
  if (step === 'review' && currentList) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-3">
          <button onClick={handleReset} className="text-sm text-neutral-500 hover:text-neutral-700">← Cancelar</button>
          <h2 className="text-xl font-bold text-neutral-900">Revisar lista — {currentList.fileName}</h2>
        </div>

        <div className="grid grid-cols-2 gap-4">
          {/* Table */}
          <div className="space-y-2">
            <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium">
              {editedItems.length} itens extraídos — edite se necessário
            </p>
            <div className="border border-neutral-200 rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-neutral-50">
                  <tr>
                    <th className="px-2 py-2 text-left text-xs font-medium text-neutral-500">SKU</th>
                    <th className="px-2 py-2 text-left text-xs font-medium text-neutral-500">Cor</th>
                    <th className="px-2 py-2 text-left text-xs font-medium text-neutral-500">Tam.</th>
                    <th className="px-2 py-2 text-center text-xs font-medium text-neutral-500">Qtd</th>
                    <th className="px-2 py-2 text-left text-xs font-medium text-neutral-500">DTF</th>
                  </tr>
                </thead>
                <tbody>
                  {editedItems.map((item, idx) => (
                    <tr key={item.id} className={idx % 2 === 0 ? 'bg-white' : 'bg-neutral-50'}>
                      <td className="px-2 py-1">
                        <input
                          value={item.sku}
                          onChange={(e) => setEditedItems((prev) =>
                            prev.map((i) => i.id === item.id ? { ...i, sku: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-neutral-300 rounded px-1"
                        />
                      </td>
                      <td className="px-2 py-1">
                        <input
                          value={item.color}
                          onChange={(e) => setEditedItems((prev) =>
                            prev.map((i) => i.id === item.id ? { ...i, color: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-neutral-300 rounded px-1"
                        />
                      </td>
                      <td className="px-2 py-1">
                        <input
                          value={item.size}
                          onChange={(e) => setEditedItems((prev) =>
                            prev.map((i) => i.id === item.id ? { ...i, size: e.target.value } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-neutral-300 rounded px-1 uppercase"
                        />
                      </td>
                      <td className="px-2 py-1 text-center">
                        <input
                          type="number"
                          min="1"
                          value={item.quantity}
                          onChange={(e) => setEditedItems((prev) =>
                            prev.map((i) => i.id === item.id ? { ...i, quantity: parseInt(e.target.value) || 1 } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-neutral-300 rounded px-1 text-center"
                        />
                      </td>
                      <td className="px-2 py-1">
                        <select
                          value={item.dtfModelId ?? ''}
                          onChange={(e) => setEditedItems((prev) =>
                            prev.map((i) => i.id === item.id ? { ...i, dtfModelId: e.target.value || null, dtfModelName: dtfModels.find((m: DtfModelDto) => m.id === e.target.value)?.name ?? null } : i))}
                          className="text-xs border border-neutral-200 rounded px-1 py-0.5 bg-white w-full"
                        >
                          <option value="">—</option>
                          {dtfModels.map((m: DtfModelDto) => (
                            <option key={m.id} value={m.id}>{m.name}</option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex gap-2 pt-2">
              <Button
                variant="outline"
                onClick={() => setEditedItems((prev) => [...prev, {
                  id: crypto.randomUUID(),
                  sku: '', color: '', size: '', quantity: 1, dtfModelId: null, dtfModelName: null, sortOrder: prev.length
                }])}
                className="text-xs h-7 px-2"
              >
                + Linha
              </Button>
              <Button
                onClick={handleSaveAndCheck}
                disabled={updateItems.isPending || editedItems.length === 0}
                className="flex-1"
              >
                {updateItems.isPending ? 'Salvando...' : 'Verificar estoque →'}
              </Button>
            </div>
            {updateItems.isError && (
              <p className="text-xs text-red-600">
                {(updateItems.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao salvar.'}
              </p>
            )}
          </div>

          {/* PDF Preview */}
          <div className="space-y-2">
            <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium">Preview do PDF</p>
            {pdfBlobUrl ? (
              <iframe
                src={pdfBlobUrl}
                className="w-full rounded-lg border border-neutral-200"
                style={{ height: '500px' }}
                title="PDF preview"
              />
            ) : (
              <div className="w-full h-64 rounded-lg border border-neutral-200 bg-neutral-50 flex items-center justify-center">
                <p className="text-xs text-neutral-400">Preview indisponível</p>
              </div>
            )}
          </div>
        </div>
      </div>
    )
  }

  // ── STOCK CHECK STEP ───────────────────────────────────────────────────────
  if (step === 'stock-check' && currentList) {
    const check = stockCheckQuery.data ?? stockCheck

    return (
      <div className="space-y-5">
        <div className="flex items-center gap-3">
          <button onClick={() => setStep('review')} className="text-sm text-neutral-500 hover:text-neutral-700">← Editar</button>
          <h2 className="text-xl font-bold text-neutral-900">Verificação de estoque</h2>
        </div>

        {stockCheckQuery.isLoading && <p className="text-sm text-neutral-500">Verificando estoque...</p>}

        {check && (
          <>
            {/* Shirts */}
            <div>
              <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-2">
                Estoque de camisetas
              </p>
              <div className="border border-neutral-200 rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-neutral-50">
                    <tr>
                      <th className="px-3 py-2 text-left text-xs font-medium text-neutral-500">Cor</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-neutral-500">Tam.</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-neutral-500">Necessário</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-neutral-500">Disponível</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-neutral-500">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {check.shirtChecks.map((c, i) => (
                      <tr key={i} className={i % 2 === 0 ? 'bg-white' : 'bg-neutral-50'}>
                        <td className="px-3 py-2 text-sm">{c.color}</td>
                        <td className="px-3 py-2 text-sm font-medium">{c.size}</td>
                        <td className="px-3 py-2 text-center text-sm">{c.needed}</td>
                        <td className="px-3 py-2 text-center text-sm">{c.available}</td>
                        <td className="px-3 py-2 text-center">
                          {c.ok
                            ? <span className="text-xs font-medium text-green-700 bg-green-50 border border-green-200 px-2 py-0.5 rounded-full">✓ OK</span>
                            : <span className="text-xs font-medium text-red-700 bg-red-50 border border-red-200 px-2 py-0.5 rounded-full">⚠ Insuficiente</span>
                          }
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {!check.canConfirm && (
                <p className="text-xs text-red-600 mt-1">
                  Estoque insuficiente em alguns itens. Edite as quantidades antes de confirmar.
                </p>
              )}
            </div>

            {/* DTF */}
            {check.dtfChecks.length > 0 && (
              <div>
                <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-2">
                  Estoque DTF
                </p>
                <div className="space-y-2">
                  {check.dtfChecks.map((d) => (
                    <div
                      key={d.dtfModelId}
                      className={`border rounded-lg p-3 text-sm ${d.fromStock ? 'border-green-200 bg-green-50' : 'border-amber-200 bg-amber-50'}`}
                    >
                      <div className="flex justify-between items-center">
                        <span className="font-semibold">{d.modelName}</span>
                        {d.fromStock
                          ? <span className="text-xs font-medium text-green-700 bg-green-100 px-2 py-0.5 rounded-full">Do estoque</span>
                          : <span className="text-xs font-medium text-amber-700 bg-amber-100 px-2 py-0.5 rounded-full">Pedir folha</span>
                        }
                      </div>
                      <div className="text-xs mt-1 text-neutral-600 space-y-0.5">
                        <p>Precisa: <b>{d.needed}</b> — Tem: <b>{d.available}</b></p>
                        {!d.fromStock && (
                          <p className="text-amber-700">
                            Pedir {d.sheetsToOrder} folha(s) × {d.stampsPerSheet} estampas = {d.stampsFromSheets} estampas
                            {d.surplus > 0 && ` (sobrará ${d.surplus})`}
                            {' '}— R$ {fmt(d.orderCost)}
                          </p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
                {check.totalDtfCost > 0 && (
                  <p className="text-sm font-medium text-neutral-700 mt-2">
                    Total DTF a comprar: <span className="font-bold">R$ {fmt(check.totalDtfCost)}</span>
                  </p>
                )}
              </div>
            )}

            <div className="flex gap-2 pt-2">
              <Button variant="outline" onClick={() => setStep('review')} className="flex-1">
                ← Editar lista
              </Button>
              <Button
                onClick={() => setStep('confirm-modal')}
                disabled={!check.canConfirm}
                className="flex-1"
              >
                Confirmar lista →
              </Button>
            </div>
          </>
        )}
      </div>
    )
  }

  // ── CONFIRM MODAL ──────────────────────────────────────────────────────────
  if (step === 'confirm-modal' && currentList && stockCheckQuery.data) {
    const check = stockCheckQuery.data

    return (
      <div className="space-y-5">
        <h2 className="text-xl font-bold text-neutral-900">Confirmar lista de separação</h2>

        <div className="rounded-lg border border-neutral-200 bg-neutral-50 p-4 space-y-4 text-sm">
          <div>
            <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-2">Desconto de estoque (camisetas)</p>
            <div className="flex flex-wrap gap-1.5">
              {check.shirtChecks.map((c, i) => (
                <span key={i} className="bg-white border border-neutral-300 rounded px-2 py-0.5 text-xs">
                  {c.color} {c.size} × {c.needed}
                </span>
              ))}
            </div>
          </div>

          {check.dtfChecks.filter(d => d.fromStock).length > 0 && (
            <div>
              <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-2">DTF do estoque</p>
              {check.dtfChecks.filter(d => d.fromStock).map((d) => (
                <p key={d.dtfModelId} className="text-xs text-green-700">{d.modelName} — {d.needed} un. do estoque</p>
              ))}
            </div>
          )}

          {check.dtfChecks.filter(d => !d.fromStock).length > 0 && (
            <div>
              <p className="text-xs text-neutral-500 uppercase tracking-wide font-medium mb-2">Folhas DTF a pedir</p>
              {check.dtfChecks.filter(d => !d.fromStock).map((d) => (
                <p key={d.dtfModelId} className="text-xs text-amber-700">
                  {d.modelName} — {d.sheetsToOrder} folha(s) — R$ {fmt(d.orderCost)}
                </p>
              ))}
              <p className="text-sm font-semibold text-neutral-800 mt-1">Total: R$ {fmt(check.totalDtfCost)}</p>
            </div>
          )}
        </div>

        <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700 font-medium">
          ⚠ Esta ação é irreversível. O estoque será descontado e os pedidos DTF lançados.
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('stock-check')} disabled={confirm.isPending} className="flex-1">
            ← Voltar
          </Button>
          <Button onClick={handleConfirm} disabled={confirm.isPending} className="flex-1 bg-red-600 hover:bg-red-700">
            {confirm.isPending ? 'Confirmando...' : 'Confirmar definitivamente'}
          </Button>
        </div>

        {confirm.isError && (
          <p className="text-xs text-red-600">
            {(confirm.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao confirmar.'}
          </p>
        )}
      </div>
    )
  }

  // ── DONE STEP ──────────────────────────────────────────────────────────────
  if (step === 'done' && confirmResult) {
    return (
      <div className="space-y-5">
        <div className="rounded-md bg-green-50 border border-green-200 p-4">
          <p className="font-semibold text-green-800 text-sm">✓ Lista confirmada com sucesso!</p>
          <p className="text-xs text-green-700 mt-1">
            {confirmResult.shirtDeductions.reduce((a, d) => a + d.quantity, 0)} peças descontadas do estoque.
          </p>
        </div>

        {confirmResult.dtfOrders.length > 0 && confirmResult.whatsAppMessage && (
          <div className="space-y-3">
            <p className="text-sm font-medium text-neutral-700">Mensagem para o fornecedor DTF</p>

            <div className="flex items-center gap-3 p-3 bg-green-50 border border-green-200 rounded-md">
              <div className="w-9 h-9 rounded-full bg-green-500 flex items-center justify-center text-white font-bold text-base shrink-0">
                {confirmResult.dtfSupplierName.charAt(0).toUpperCase()}
              </div>
              <div>
                <p className="text-sm font-semibold text-green-900">{confirmResult.dtfSupplierName}</p>
                <p className="text-xs text-green-700">{confirmResult.dtfSupplierPhone || 'Número não configurado'}</p>
              </div>
            </div>

            <div>
              <div className="flex justify-between items-center mb-1">
                <label className="text-sm font-medium text-neutral-700">Mensagem WhatsApp</label>
                <button
                  onClick={() => handleCopy(confirmResult.whatsAppMessage!)}
                  className={`text-xs px-2 py-1 rounded font-medium transition-colors ${copied ? 'bg-green-100 text-green-700' : 'bg-neutral-100 text-neutral-600 hover:bg-neutral-200'}`}
                >
                  {copied ? '✓ Copiado!' : 'Copiar'}
                </button>
              </div>
              <pre className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm font-mono bg-neutral-50 whitespace-pre-wrap">
                {confirmResult.whatsAppMessage}
              </pre>
            </div>

            {confirmResult.waMeLink && (
              <a
                href={confirmResult.waMeLink}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center justify-center gap-2 w-full rounded-md px-4 py-2 text-sm font-medium bg-[#25D366] text-white hover:bg-[#1ebe5a] transition-colors"
              >
                <svg viewBox="0 0 24 24" className="w-4 h-4 fill-current" aria-hidden>
                  <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z" />
                  <path d="M12 0C5.373 0 0 5.373 0 12c0 2.123.554 4.118 1.525 5.847L0 24l6.293-1.498A11.954 11.954 0 0012 24c6.627 0 12-5.373 12-12S18.627 0 12 0zm0 21.818a9.793 9.793 0 01-5.001-1.372l-.358-.213-3.728.887.934-3.617-.233-.371A9.787 9.787 0 012.182 12C2.182 6.58 6.58 2.182 12 2.182S21.818 6.58 21.818 12 17.42 21.818 12 21.818z" />
                </svg>
                Abrir no WhatsApp
              </a>
            )}
          </div>
        )}

        <Button onClick={handleReset} className="w-full">Voltar para a lista</Button>
      </div>
    )
  }

  return null
}
