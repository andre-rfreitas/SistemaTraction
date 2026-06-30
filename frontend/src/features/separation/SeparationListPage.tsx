import { useState, useRef } from 'react'
import { useSeparationLists } from './hooks/useSeparationLists'
import { useUploadSeparationList } from './hooks/useUploadSeparationList'
import { useUpdateSeparationItems } from './hooks/useUpdateSeparationItems'
import { useStockCheck } from './hooks/useStockCheck'
import { useConfirmSeparationList } from './hooks/useConfirmSeparationList'
import { useDeleteSeparationList } from './hooks/useDeleteSeparationList'
import { useRenameSeparationList } from './hooks/useRenameSeparationList'
import { useSupplyDeductionPreview } from './hooks/useSupplyDeductionPreview'
import { useDeductSuppliesForSeparation } from './hooks/useDeductSuppliesForSeparation'
import { SkuConfigPanel } from './components/SkuConfigPanel'
import type {
  SeparationListSummary,
  SeparationListDetail,
  SeparationItemDto,
  SeparationConfirmResult,
} from './types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { ClipboardList } from 'lucide-react'

type MainTab = 'lists' | 'config'

// ── Shell ──────────────────────────────────────────────────────────────────────
export function SeparationListPage() {
  const [tab, setTab] = useState<MainTab>('lists')

  const tabBtn = (t: MainTab, label: string) => (
    <button
      key={t}
      onClick={() => setTab(t)}
      className={`px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors ${
        tab === t
          ? 'border-foreground text-foreground'
          : 'border-transparent text-muted-foreground hover:text-foreground'
      }`}
    >
      {label}
    </button>
  )

  return (
    <div className="space-y-4">
      <div className="flex gap-1 border-b border-border">
        {tabBtn('lists', 'Listas')}
        {tabBtn('config', 'Config. SKU')}
      </div>

      {tab === 'lists'  && <SeparationWizard />}
      {tab === 'config' && <SkuConfigPanel />}
    </div>
  )
}

// ── Wizard ─────────────────────────────────────────────────────────────────────
type Step = 'list' | 'upload' | 'review' | 'stock-check' | 'supply-deduction' | 'confirm-modal' | 'done'

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Pendente', Confirmed: 'Confirmada', Cancelled: 'Cancelada',
}
const STATUS_VARIANT: Record<string, 'warning' | 'success' | 'danger' | 'neutral'> = {
  Pending: 'warning',
  Confirmed: 'success',
  Cancelled: 'danger',
}

function SeparationWizard() {
  const [step, setStep] = useState<Step>('list')
  const [currentList, setCurrentList] = useState<SeparationListDetail | null>(null)
  const [editedItems, setEditedItems] = useState<SeparationItemDto[]>([])
  const [confirmResult, setConfirmResult] = useState<SeparationConfirmResult | null>(null)
  const [pdfBlobUrl, setPdfBlobUrl] = useState<string | null>(null)
  const [supplyDeductions, setSupplyDeductions] = useState<{ supplyStockItemId: string; quantity: number; name: string; unit: string }[]>([])
  const fileInputRef = useRef<HTMLInputElement>(null)

  // All hooks called unconditionally
  const { data: lists = [], isLoading } = useSeparationLists()
  const upload = useUploadSeparationList()
  const updateItems = useUpdateSeparationItems()
  const stockCheckQuery = useStockCheck(step === 'stock-check' || step === 'supply-deduction' || step === 'confirm-modal' ? currentList?.id ?? null : null)
  const totalOrders = currentList?.items.reduce((sum, i) => sum + (i.quantity ?? 0), 0) ?? 0
  const supplyPreviewQuery = useSupplyDeductionPreview(step === 'supply-deduction' ? totalOrders : null)
  const deductSupplies = useDeductSuppliesForSeparation()
  const confirm = useConfirmSeparationList()

  function handleFileSelect(file: File) {
    setPdfBlobUrl(URL.createObjectURL(file))
    upload.mutate(file, {
      onSuccess: (list) => { setCurrentList(list); setEditedItems([...list.items]); setStep('review') },
    })
  }

  function handleSaveAndCheck() {
    if (!currentList) return
    updateItems.mutate(
      { listId: currentList.id, items: editedItems },
      { onSuccess: (updated) => { setCurrentList(updated); setEditedItems([...updated.items]); setStep('stock-check') } },
    )
  }

  function handleConfirm() {
    if (!currentList) return
    confirm.mutate(currentList.id, {
      onSuccess: (result) => {
        if (supplyDeductions.length > 0) {
          deductSupplies.mutate(
            supplyDeductions.filter(d => d.quantity > 0),
            { onSuccess: () => { setConfirmResult(result); setStep('done') } }
          )
        } else {
          setConfirmResult(result)
          setStep('done')
        }
      },
    })
  }

  function handleReset() {
    if (pdfBlobUrl) URL.revokeObjectURL(pdfBlobUrl)
    setPdfBlobUrl(null); setCurrentList(null); setEditedItems([])
    setConfirmResult(null); setSupplyDeductions([]); setStep('list')
  }

  if (step === 'list') {
    return (
      <ListStep
        lists={lists}
        isLoading={isLoading}
        onNew={() => setStep('upload')}
      />
    )
  }


  // ── UPLOAD ────────────────────────────────────────────────────────────────
  if (step === 'upload') return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={handleReset} className="text-sm text-muted-foreground hover:text-foreground">← Voltar</button>
        <h2 className="text-xl font-bold text-foreground">Nova lista de separação</h2>
      </div>

      <div
        className="border-2 border-dashed border-border rounded-xl p-12 text-center cursor-pointer hover:border-foreground/40 transition-colors"
        onClick={() => fileInputRef.current?.click()}
        onDragOver={(e) => e.preventDefault()}
        onDrop={(e) => {
          e.preventDefault()
          const file = e.dataTransfer.files[0]
          if (file?.type === 'application/pdf') handleFileSelect(file)
        }}
      >
        <div className="text-4xl mb-3">📄</div>
        <p className="text-sm font-medium text-foreground">Clique ou arraste o PDF aqui</p>
        <p className="text-xs text-muted-foreground mt-1">Apenas arquivos .pdf do ERP</p>
        <input ref={fileInputRef} type="file" accept=".pdf" className="hidden"
          onChange={(e) => { const f = e.target.files?.[0]; if (f) handleFileSelect(f) }} />
      </div>

      {upload.isPending && <p className="text-sm text-muted-foreground text-center">Processando PDF...</p>}
      {upload.isError && (
        <div className="rounded-md bg-danger/10 border border-danger/20 p-3 text-sm text-danger">
          {(upload.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao processar o PDF.'}
        </div>
      )}
    </div>
  )

  // ── REVIEW ────────────────────────────────────────────────────────────────
  if (step === 'review' && currentList) return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <button onClick={handleReset} className="text-sm text-muted-foreground hover:text-foreground">← Cancelar</button>
        <h2 className="text-xl font-bold text-foreground">Revisar lista — {currentList.fileName}</h2>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
            {editedItems.length} itens extraídos — edite se necessário
          </p>
          <div className="border border-border rounded-lg overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted">
                <tr>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">SKU</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Modelagem</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Estampa</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Cor</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Tam.</th>
                  <th className="px-2 py-2 text-center text-xs font-medium text-muted-foreground">Qtd</th>
                </tr>
              </thead>
              <tbody>
                {editedItems.map((item, idx) => {
                  const modelCode = item.sku.split('-')[0] ?? ''
                  return (
                    <tr key={item.id} className={idx % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                      <td className="px-2 py-1">
                        <input value={item.sku}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, sku: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 font-mono" />
                      </td>
                      <td className="px-2 py-1">
                        <span className="text-xs font-mono text-muted-foreground">{modelCode || '—'}</span>
                      </td>
                      <td className="px-2 py-1">
                        <span className="text-xs text-muted-foreground">{item.estampa || '—'}</span>
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.color}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, color: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1" />
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.size}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, size: e.target.value.toUpperCase() } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 uppercase" />
                      </td>
                      <td className="px-2 py-1 text-center">
                        <input type="number" min="1" value={item.quantity}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, quantity: parseInt(e.target.value) || 1 } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 text-center" />
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>

          <div className="flex gap-2 pt-2">
            <Button variant="outline" className="text-xs h-7 px-2"
              onClick={() => setEditedItems(p => [...p, {
                id: crypto.randomUUID(), sku: '', estampa: '', color: '', size: '', quantity: 1, sortOrder: p.length, dtfModelId: null
              }])}>
              + Linha
            </Button>
            <Button onClick={handleSaveAndCheck} disabled={updateItems.isPending || editedItems.length === 0} className="flex-1">
              {updateItems.isPending ? 'Salvando...' : 'Verificar estoque →'}
            </Button>
          </div>
          {updateItems.isError && (
            <p className="text-xs text-danger">
              {(updateItems.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao salvar.'}
            </p>
          )}
        </div>

        <div className="space-y-2">
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Preview do PDF</p>
          {pdfBlobUrl
            ? <iframe src={pdfBlobUrl} className="w-full rounded-lg border border-border" style={{ height: '500px' }} title="PDF preview" />
            : <div className="w-full h-64 rounded-lg border border-border bg-muted flex items-center justify-center">
                <p className="text-xs text-muted-foreground">Preview indisponível</p>
              </div>
          }
        </div>
      </div>
    </div>
  )

  // ── STOCK CHECK ───────────────────────────────────────────────────────────
  if (step === 'stock-check' && currentList) {
    const check = stockCheckQuery.data
    return (
      <div className="space-y-5">
        <div className="flex items-center gap-3">
          <button onClick={() => setStep('review')} className="text-sm text-muted-foreground hover:text-foreground">← Editar</button>
          <h2 className="text-xl font-bold text-foreground">Verificação de estoque</h2>
        </div>

        {stockCheckQuery.isLoading && <p className="text-sm text-muted-foreground">Verificando estoque...</p>}

        {check && (
          <>
            <div>
              <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-2">Estoque de camisetas</p>
              <div className="border border-border rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-muted">
                    <tr>
                      <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Modelo</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Cor</th>
                      <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Tam.</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-muted-foreground">Necessário</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-muted-foreground">Disponível</th>
                      <th className="px-3 py-2 text-center text-xs font-medium text-muted-foreground">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {check.shirtChecks.map((c, i) => (
                      <tr key={i} className={i % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                        <td className="px-3 py-2 font-mono text-muted-foreground">{c.modelCode}</td>
                        <td className="px-3 py-2">{c.color}</td>
                        <td className="px-3 py-2 font-medium">{c.size}</td>
                        <td className="px-3 py-2 text-center">{c.needed}</td>
                        <td className="px-3 py-2 text-center">{c.available}</td>
                        <td className="px-3 py-2 text-center">
                          {c.ok
                            ? <Badge variant="success">✓ OK</Badge>
                            : <Badge variant="danger">⚠ Insuficiente</Badge>
                          }
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {!check.canConfirm && <p className="text-xs text-danger mt-1">Estoque insuficiente. Edite as quantidades antes de confirmar.</p>}
            </div>

            <div className="flex gap-2 pt-2">
              <Button variant="outline" onClick={() => setStep('review')} className="flex-1">← Editar lista</Button>
              <Button onClick={() => setStep('supply-deduction')} disabled={!check.canConfirm} className="flex-1">
                Confirmar lista →
              </Button>
            </div>
          </>
        )}
      </div>
    )
  }

  // ── SUPPLY DEDUCTION ──────────────────────────────────────────────────────
  if (step === 'supply-deduction') {
    const preview = supplyPreviewQuery.data ?? []

    if (supplyPreviewQuery.isLoading) return (
      <div className="space-y-4">
        <h2 className="text-xl font-bold text-foreground">Desconto de embalagens</h2>
        <p className="text-sm text-muted-foreground">Calculando...</p>
      </div>
    )

    const editableDeductions = supplyDeductions.length > 0
      ? supplyDeductions
      : preview.map(p => ({ supplyStockItemId: p.supplyStockItemId, quantity: p.totalQuantity, name: p.name, unit: p.unit }))

    function updateQty(supplyStockItemId: string, qty: number) {
      setSupplyDeductions(
        (editableDeductions.length > 0 ? editableDeductions : preview.map(p => ({
          supplyStockItemId: p.supplyStockItemId, quantity: p.totalQuantity, name: p.name, unit: p.unit
        }))).map(d => d.supplyStockItemId === supplyStockItemId ? { ...d, quantity: qty } : d)
      )
    }

    return (
      <div className="space-y-5">
        <div className="flex items-center gap-3">
          <button onClick={() => setStep('stock-check')} className="text-sm text-muted-foreground hover:text-foreground">← Voltar</button>
          <h2 className="text-xl font-bold text-foreground">Desconto de embalagens</h2>
        </div>

        {preview.length === 0 ? (
          <div className="rounded-md bg-muted p-4 text-sm text-muted-foreground">
            Nenhum insumo configurado por pedido. Configure em Configurações → Embalagens.
          </div>
        ) : (
          <div className="space-y-2">
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">
              Insumos a descontar ({totalOrders} pedido{totalOrders !== 1 ? 's' : ''})
            </p>
            <div className="border border-border rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted">
                  <tr>
                    <th className="px-3 py-2 text-left text-xs font-medium text-muted-foreground">Insumo</th>
                    <th className="px-3 py-2 text-center text-xs font-medium text-muted-foreground">Qtd por pedido</th>
                    <th className="px-3 py-2 text-center text-xs font-medium text-muted-foreground">Total</th>
                  </tr>
                </thead>
                <tbody>
                  {(editableDeductions.length > 0 ? editableDeductions : preview.map(p => ({
                    supplyStockItemId: p.supplyStockItemId, quantity: p.totalQuantity, name: p.name, unit: p.unit
                  }))).map((d, i) => (
                    <tr key={d.supplyStockItemId} className={i % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                      <td className="px-3 py-2 font-medium">{d.name}</td>
                      <td className="px-3 py-2 text-center text-muted-foreground">
                        {preview.find(p => p.supplyStockItemId === d.supplyStockItemId)?.quantityPerOrder ?? '—'} {d.unit}
                      </td>
                      <td className="px-3 py-2 text-center">
                        <input
                          type="number"
                          min="0"
                          value={d.quantity}
                          onChange={(e) => updateQty(d.supplyStockItemId, parseInt(e.target.value) || 0)}
                          className="w-16 text-sm border border-input rounded px-2 py-1 text-center bg-background"
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('stock-check')} className="flex-1">← Voltar</Button>
          <Button
            onClick={() => {
              if (editableDeductions.length === 0 && preview.length > 0) {
                setSupplyDeductions(preview.map(p => ({
                  supplyStockItemId: p.supplyStockItemId, quantity: p.totalQuantity, name: p.name, unit: p.unit
                })))
              }
              setStep('confirm-modal')
            }}
            className="flex-1"
          >
            Continuar →
          </Button>
        </div>
      </div>
    )
  }

  // ── CONFIRM MODAL ─────────────────────────────────────────────────────────
  if (step === 'confirm-modal' && currentList && stockCheckQuery.data) {
    const check = stockCheckQuery.data
    return (
      <div className="space-y-5">
        <h2 className="text-xl font-bold text-foreground">Confirmar lista de separação</h2>

        <div className="rounded-lg border border-border bg-muted p-4 space-y-4 text-sm">
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium mb-2">Desconto de estoque</p>
            <div className="flex flex-wrap gap-1.5">
              {check.shirtChecks.map((c, i) => (
                <span key={i} className="bg-card border border-border rounded px-2 py-0.5 text-xs text-foreground">
                  {c.modelCode} {c.color} {c.size} × {c.needed}
                </span>
              ))}
            </div>
          </div>
        </div>

        <div className="rounded-md bg-danger/10 border border-danger/20 p-3 text-sm text-danger font-medium">
          ⚠ Esta ação é irreversível. O estoque será descontado.
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('stock-check')} disabled={confirm.isPending} className="flex-1">← Voltar</Button>
          <Button variant="destructive" onClick={handleConfirm} disabled={confirm.isPending} className="flex-1">
            {confirm.isPending ? 'Confirmando...' : 'Confirmar definitivamente'}
          </Button>
        </div>

        {confirm.isError && (
          <p className="text-xs text-danger">
            {(confirm.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao confirmar.'}
          </p>
        )}
      </div>
    )
  }

  // ── DONE ──────────────────────────────────────────────────────────────────
  if (step === 'done' && confirmResult) return (
    <div className="space-y-5">
      <div className="rounded-md bg-success/10 border border-success/20 p-4">
        <p className="font-semibold text-success text-sm">✓ Lista confirmada com sucesso!</p>
        <p className="text-xs text-success mt-1">
          {confirmResult.shirtDeductions.reduce((a, d) => a + d.quantity, 0)} peças descontadas do estoque.
        </p>
      </div>

      <Button onClick={handleReset} className="w-full">Voltar para a lista</Button>
    </div>
  )

  return null
}

// ── ListStep ────────────────────────────────────────────────────────────────
function ListStep({
  lists,
  isLoading,
  onNew,
}: {
  lists: SeparationListSummary[]
  isLoading: boolean
  onNew: () => void
}) {
  const deleteMutation = useDeleteSeparationList()
  const renameMutation = useRenameSeparationList()

  const [deleteTarget, setDeleteTarget] = useState<SeparationListSummary | null>(null)
  const [editTarget, setEditTarget] = useState<SeparationListSummary | null>(null)
  const [editName, setEditName] = useState('')

  function startEdit(l: SeparationListSummary) {
    setEditTarget(l)
    setEditName(l.fileName)
  }

  function cancelEdit() {
    setEditTarget(null)
    setEditName('')
  }

  function submitRename() {
    if (!editTarget || !editName.trim()) return
    renameMutation.mutate(
      { id: editTarget.id, fileName: editName.trim() },
      { onSuccess: cancelEdit },
    )
  }

  function confirmDelete() {
    if (!deleteTarget) return
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => setDeleteTarget(null),
    })
  }

  const canEdit = (l: SeparationListSummary) => l.status !== 'Confirmed'

  return (
    <>
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <div>
            <h2 className="text-xl font-bold text-foreground">Lista de Separação</h2>
            <p className="text-sm text-muted-foreground">Importar PDF do ERP e processar pedidos.</p>
          </div>
          <Button onClick={onNew}>+ Nova lista</Button>
        </div>

        {isLoading && (
          <div className="space-y-2">
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
          </div>
        )}
        {!isLoading && lists.length === 0 && (
          <EmptyState
            icon={ClipboardList}
            title="Nenhuma lista importada ainda."
            description="Clique em '+ Nova lista' para importar um PDF do ERP."
          />
        )}

        <div className="space-y-2">
          {lists.map((l) => (
            <div key={l.id} className="border border-border rounded-lg p-4 bg-card">
              {editTarget?.id === l.id ? (
                // ── inline edit mode ──
                <div className="flex items-center gap-2">
                  <input
                    className="flex-1 text-sm border border-input rounded px-2 py-1 focus:outline-none focus:ring-2 focus:ring-ring bg-background text-foreground"
                    value={editName}
                    autoFocus
                    onChange={(e) => setEditName(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') submitRename()
                      if (e.key === 'Escape') cancelEdit()
                    }}
                  />
                  <button
                    onClick={submitRename}
                    disabled={renameMutation.isPending}
                    className="text-xs px-3 py-1 rounded bg-foreground text-background hover:bg-foreground/80 disabled:opacity-50 transition-colors"
                  >
                    {renameMutation.isPending ? '...' : 'Salvar'}
                  </button>
                  <button
                    onClick={cancelEdit}
                    className="text-xs px-2 py-1 rounded border border-border text-muted-foreground hover:bg-muted transition-colors"
                  >
                    Cancelar
                  </button>
                </div>
              ) : (
                // ── normal display mode ──
                <div className="flex justify-between items-center gap-3">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-foreground truncate">{l.fileName}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {new Date(l.uploadedAt).toLocaleDateString('pt-BR')} — {l.itemCount} itens — {l.totalQuantity} peças
                    </p>
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    <Badge variant={STATUS_VARIANT[l.status] ?? 'neutral'}>
                      {STATUS_LABEL[l.status] ?? l.status}
                    </Badge>

                    {canEdit(l) && (
                      <>
                        <button
                          title="Editar nome"
                          onClick={() => startEdit(l)}
                          className="p-1.5 rounded hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                        >
                          {/* pencil icon */}
                          <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                          </svg>
                        </button>
                        <button
                          title="Excluir lista"
                          onClick={() => setDeleteTarget(l)}
                          className="p-1.5 rounded hover:bg-danger/10 text-muted-foreground hover:text-danger transition-colors"
                        >
                          {/* trash icon */}
                          <svg xmlns="http://www.w3.org/2000/svg" className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <polyline points="3 6 5 6 21 6" />
                            <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
                            <path d="M10 11v6" />
                            <path d="M14 11v6" />
                            <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
                          </svg>
                        </button>
                      </>
                    )}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* ── Delete confirmation modal ── */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <div className="bg-card rounded-xl shadow-2xl p-6 w-full max-w-sm mx-4 space-y-4">
            <div className="flex items-start gap-3">
              <div className="w-9 h-9 rounded-full bg-danger/10 flex items-center justify-center shrink-0">
                <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5 text-danger" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="3 6 5 6 21 6" />
                  <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
                  <path d="M10 11v6" /><path d="M14 11v6" />
                  <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
                </svg>
              </div>
              <div>
                <p className="text-sm font-semibold text-foreground">Excluir lista?</p>
                <p className="text-xs text-muted-foreground mt-1">
                  <span className="font-medium">{deleteTarget.fileName}</span> será removida permanentemente. Esta ação não pode ser desfeita.
                </p>
              </div>
            </div>

            {deleteMutation.isError && (
              <p className="text-xs text-danger">
                {(deleteMutation.error as { response?: { data?: { error?: string } } })?.response?.data?.error ?? 'Erro ao excluir.'}
              </p>
            )}

            <div className="flex gap-2 pt-1">
              <button
                onClick={() => setDeleteTarget(null)}
                disabled={deleteMutation.isPending}
                className="flex-1 text-sm px-3 py-2 rounded-lg border border-border text-foreground hover:bg-muted transition-colors disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                onClick={confirmDelete}
                disabled={deleteMutation.isPending}
                className="flex-1 text-sm px-3 py-2 rounded-lg bg-danger text-white font-medium hover:bg-danger/90 transition-colors disabled:opacity-50"
              >
                {deleteMutation.isPending ? 'Excluindo...' : 'Excluir'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
