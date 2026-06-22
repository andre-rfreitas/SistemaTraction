import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Trash2, Pencil, Plus, AlertTriangle, Package } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useFabricTypes } from '@/features/settings/fabric/hooks/useFabricTypes'
import { useAdjustShirtStock, type AdjustPayload } from '../hooks/useAdjustShirtStock'
import { useShirtStockItems, type ShirtStockItemDto } from '../hooks/useShirtStockItems'
import { useDeleteShirtStockItem } from '../hooks/useDeleteShirtStockItem'

import { cn } from '@/lib/utils'

type Tab = 'cadastrar' | 'editar' | 'excluir'

interface CadastrarForm {
  fabricColorId: string
  size: string
  modelCode: string
  quantity: string
  unitCost: string
  reason: string
}

const SIZES = ['P', 'M', 'G', 'GG', 'G1', 'G2', 'G3', 'XGG']

const EMPTY_FORM: CadastrarForm = {
  fabricColorId: '',
  size: '',
  modelCode: 'REG',
  quantity: '',
  unitCost: '',
  reason: 'Cadastro manual',
}

interface Props {
  open: boolean
  onClose: () => void
  currentModelCode: string
}

export function ManageProductsModal({ open, onClose, currentModelCode }: Props) {
  const [tab, setTab] = useState<Tab>('cadastrar')
  const [form, setForm] = useState<CadastrarForm>({ ...EMPTY_FORM, modelCode: currentModelCode })
  const [confirmStep, setConfirmStep] = useState(false)
  const [editItem, setEditItem] = useState<ShirtStockItemDto | null>(null)
  const [editQty, setEditQty] = useState('')
  const [editUnitCost, setEditUnitCost] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<ShirtStockItemDto | null>(null)
  const [errorMsg, setErrorMsg] = useState('')
  const [successMsg, setSuccessMsg] = useState('')

  const queryClient = useQueryClient()
  const { data: fabricTypes = [] } = useFabricTypes()
  const { data: stockItems = [], isLoading: itemsLoading } = useShirtStockItems(currentModelCode)
  const adjust = useAdjustShirtStock()
  const deleteItem = useDeleteShirtStockItem()

  const allColors = fabricTypes.flatMap((t) =>
    t.colors.map((c) => ({ id: c.id, label: `${c.name} — ${t.name} ${t.variation}`, name: c.name }))
  )

  const setField = (key: keyof CadastrarForm, value: string) =>
    setForm((f) => ({ ...f, [key]: value }))

  const qty = parseInt(form.quantity) || 0
  const unitCost = parseFloat(form.unitCost.replace(',', '.')) || 0
  const totalCost = qty * unitCost

  const isCadastrarValid =
    form.fabricColorId && form.size && qty > 0 && form.reason.trim()

  function resetAll() {
    setForm({ ...EMPTY_FORM, modelCode: currentModelCode })
    setConfirmStep(false)
    setEditItem(null)
    setEditQty('')
    setEditUnitCost('')
    setDeleteTarget(null)
    setErrorMsg('')
    setSuccessMsg('')
  }

  function handleTabChange(t: Tab) {
    setTab(t)
    resetAll()
  }

  function handleClose() {
    resetAll()
    onClose()
  }

  async function handleCadastrar() {
    if (!isCadastrarValid) return
    setErrorMsg('')
    try {
      const payload: AdjustPayload = {
        fabricColorId: form.fabricColorId,
        size: form.size,
        adjustmentType: 'Entrada',
        quantity: qty,
        reason: form.reason,
        modelCode: form.modelCode,
        unitCost: unitCost > 0 ? unitCost : undefined,
      }
      await adjust.mutateAsync(payload)
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock-items'] })
      const selectedColor = allColors.find((c) => c.id === form.fabricColorId)
      setSuccessMsg(`✓ ${qty} un. de ${selectedColor?.name} ${form.size} cadastradas com sucesso!`)
      setForm({ ...EMPTY_FORM, modelCode: currentModelCode })
      setConfirmStep(false)
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao cadastrar produto.')
      setConfirmStep(false)
    }
  }

  async function handleEditar() {
    if (!editItem) return
    const newQty = parseInt(editQty) || 0
    const uc = parseFloat(editUnitCost.replace(',', '.')) || 0
    const delta = newQty - editItem.quantity
    if (delta === 0) { setEditItem(null); setEditQty(''); setEditUnitCost(''); return }
    setErrorMsg('')
    try {
      const payload: AdjustPayload = {
        fabricColorId: editItem.fabricColorId,
        size: editItem.size,
        adjustmentType: delta > 0 ? 'Entrada' : 'Saída',
        quantity: Math.abs(delta),
        reason: 'Edição manual de estoque',
        modelCode: editItem.modelCode,
        unitCost: delta > 0 && uc > 0 ? uc : undefined,
      }
      await adjust.mutateAsync(payload)
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock-items'] })
      setSuccessMsg(`✓ ${editItem.fabricColorName} ${editItem.size} atualizado para ${newQty} un.`)
      setEditItem(null)
      setEditQty('')
      setEditUnitCost('')
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao editar item.')
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    setErrorMsg('')
    try {
      await deleteItem.mutateAsync(deleteTarget.id)
      await queryClient.invalidateQueries({ queryKey: ['shirt-stock-items'] })
      setSuccessMsg(`✓ ${deleteTarget.fabricColorName} ${deleteTarget.size} removido do estoque.`)
      setDeleteTarget(null)
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao excluir item.')
    }
  }

  const selectedColor = allColors.find((c) => c.id === form.fabricColorId)

  const isBusy = adjust.isPending || deleteItem.isPending

  const TABS: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'cadastrar', label: 'Cadastrar', icon: <Plus className="h-3.5 w-3.5" /> },
    { id: 'editar', label: 'Editar', icon: <Pencil className="h-3.5 w-3.5" /> },
    { id: 'excluir', label: 'Excluir', icon: <Trash2 className="h-3.5 w-3.5" /> },
  ]

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Package className="h-4 w-4 text-primary" />
            Gerenciar produtos no estoque
          </DialogTitle>
        </DialogHeader>

        {/* Tabs */}
        <div className="flex gap-1 rounded-lg bg-muted/50 p-1">
          {TABS.map((t) => (
            <button
              key={t.id}
              onClick={() => handleTabChange(t.id)}
              className={cn(
                'flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-all',
                tab === t.id
                  ? 'bg-card text-foreground shadow-sm'
                  : 'text-muted-foreground hover:text-foreground'
              )}
            >
              {t.icon}
              {t.label}
            </button>
          ))}
        </div>

        {/* Feedback messages */}
        {successMsg && (
          <div className="rounded-md border border-success/30 bg-success/10 px-4 py-2.5 text-sm text-success">
            {successMsg}
          </div>
        )}
        {errorMsg && (
          <div className="flex items-start gap-2 rounded-md border border-destructive/30 bg-destructive/10 px-4 py-2.5 text-sm text-destructive">
            <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
            {errorMsg}
          </div>
        )}

        {/* ───── CADASTRAR ───── */}
        {tab === 'cadastrar' && (
          <div className="space-y-4">
            {confirmStep ? (
              /* Confirmation step */
              <div className="space-y-4">
                <div className="rounded-md border border-border bg-muted/50 p-4 space-y-2 text-sm">
                  <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Resumo do cadastro</p>
                  <div className="grid grid-cols-2 gap-y-1.5">
                    <span className="text-muted-foreground">Cor:</span>
                    <span className="font-medium">{selectedColor?.name}</span>
                    <span className="text-muted-foreground">Tamanho:</span>
                    <span className="font-medium">{form.size}</span>
                    <span className="text-muted-foreground">Modelo:</span>
                    <span className="font-medium">{form.modelCode}</span>
                    <span className="text-muted-foreground">Quantidade:</span>
                    <span className="font-bold">{qty} un.</span>
                    {unitCost > 0 && (
                      <>
                        <span className="text-muted-foreground">Custo unit.:</span>
                        <span className="font-medium">R$ {unitCost.toFixed(2)}</span>
                        <span className="text-muted-foreground">Total financeiro:</span>
                        <span className="font-bold text-destructive">R$ {totalCost.toFixed(2)}</span>
                      </>
                    )}
                    <span className="text-muted-foreground">Motivo:</span>
                    <span>{form.reason}</span>
                  </div>
                </div>
                {unitCost > 0 && (
                  <p className="text-xs text-muted-foreground">
                    Uma despesa de <strong>R$ {totalCost.toFixed(2)}</strong> será lançada automaticamente no financeiro com categoria <strong>Estoque</strong>.
                  </p>
                )}
                <div className="flex gap-2">
                  <Button variant="outline" onClick={() => setConfirmStep(false)} disabled={isBusy} className="flex-1">
                    Voltar
                  </Button>
                  <Button onClick={handleCadastrar} disabled={isBusy} className="flex-1">
                    {isBusy ? 'Cadastrando...' : 'Confirmar cadastro'}
                  </Button>
                </div>
              </div>
            ) : (
              /* Form step */
              <>
                <div>
                  <label className="block text-sm font-medium mb-1">Cor</label>
                  <select
                    value={form.fabricColorId}
                    onChange={(e) => setField('fabricColorId', e.target.value)}
                    className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  >
                    <option value="">Selecione a cor...</option>
                    {allColors.map((c) => <option key={c.id} value={c.id}>{c.label}</option>)}
                  </select>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium mb-1">Tamanho</label>
                    <select
                      value={form.size}
                      onChange={(e) => setField('size', e.target.value)}
                      className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    >
                      <option value="">Selecione...</option>
                      {SIZES.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">Modelo</label>
                    <div className="flex h-9 w-full rounded-md border border-input bg-muted px-3 py-1.5 text-sm shadow-sm opacity-70">
                      {currentModelCode}
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium mb-1">Quantidade</label>
                    <Input
                      type="number"
                      min="1"
                      value={form.quantity}
                      onChange={(e) => setField('quantity', e.target.value)}
                      placeholder="0"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">
                      Custo unit. (R$)
                      <span className="ml-1 text-xs text-muted-foreground font-normal">opcional</span>
                    </label>
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      value={form.unitCost}
                      onChange={(e) => setField('unitCost', e.target.value)}
                      placeholder="0,00"
                    />
                  </div>
                </div>

                {unitCost > 0 && qty > 0 && (
                  <div className="rounded-md bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                    Total a lançar no financeiro:{' '}
                    <span className="font-semibold text-destructive">R$ {totalCost.toFixed(2)}</span>
                    {' '}(despesa · categoria <strong>Estoque</strong>)
                  </div>
                )}

                <div>
                  <label className="block text-sm font-medium mb-1">Motivo</label>
                  <Input
                    value={form.reason}
                    onChange={(e) => setField('reason', e.target.value)}
                    placeholder="Ex: produção própria, compra de fornecedor..."
                  />
                </div>

                <div className="flex gap-2">
                  <Button variant="outline" onClick={handleClose} className="flex-1">Cancelar</Button>
                  <Button
                    onClick={() => { setErrorMsg(''); setSuccessMsg(''); setConfirmStep(true) }}
                    disabled={!isCadastrarValid}
                    className="flex-1"
                  >
                    Revisar →
                  </Button>
                </div>
              </>
            )}
          </div>
        )}

        {/* ───── EDITAR ───── */}
        {tab === 'editar' && (
          <div className="space-y-3">
            {itemsLoading ? (
              <p className="text-sm text-muted-foreground text-center py-6">Carregando itens...</p>
            ) : stockItems.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-6">
                Nenhum item em estoque para editar.
              </p>
            ) : editItem ? (
              /* Inline edit form */
              <div className="space-y-4">
                <div className="rounded-md border border-primary/30 bg-primary/5 px-4 py-3 text-sm">
                  <p className="font-medium text-foreground">{editItem.fabricColorName} — {editItem.size} ({editItem.modelCode})</p>
                  <p className="text-muted-foreground">Quantidade atual: <strong>{editItem.quantity}</strong> un.</p>
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium mb-1">Nova quantidade</label>
                    <Input
                      type="number"
                      min="0"
                      value={editQty}
                      onChange={(e) => setEditQty(e.target.value)}
                      placeholder={String(editItem.quantity)}
                      autoFocus
                    />
                    {editQty !== '' && parseInt(editQty) !== editItem.quantity && (
                      <p className={cn('text-xs mt-1', parseInt(editQty) > editItem.quantity ? 'text-success' : 'text-destructive')}>
                        {parseInt(editQty) > editItem.quantity ? '▲' : '▼'} {Math.abs((parseInt(editQty) || 0) - editItem.quantity)} un.
                      </p>
                    )}
                  </div>
                  {editQty !== '' && (parseInt(editQty) || 0) > editItem.quantity && (
                    <div>
                      <label className="block text-sm font-medium mb-1">
                        Custo unit. (R$)
                        <span className="ml-1 text-xs text-muted-foreground font-normal">opcional</span>
                      </label>
                      <Input
                        type="number"
                        min="0"
                        step="0.01"
                        value={editUnitCost}
                        onChange={(e) => setEditUnitCost(e.target.value)}
                        placeholder="0,00"
                      />
                    </div>
                  )}
                </div>
                {editUnitCost && parseFloat(editUnitCost.replace(',', '.')) > 0 && editQty && (parseInt(editQty) || 0) > editItem.quantity && (
                  <div className="rounded-md bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                    Total a lançar no financeiro:{' '}
                    <span className="font-semibold text-destructive">
                      R$ {((parseInt(editQty) - editItem.quantity) * parseFloat(editUnitCost.replace(',', '.'))).toFixed(2)}
                    </span>
                    {' '}(despesa · <strong>Estoque</strong>)
                  </div>
                )}
                <div className="flex gap-2">
                  <Button variant="outline" onClick={() => { setEditItem(null); setEditQty(''); setEditUnitCost(''); setErrorMsg('') }} disabled={isBusy} className="flex-1">
                    Cancelar
                  </Button>
                  <Button onClick={handleEditar} disabled={isBusy || !editQty || parseInt(editQty) === editItem.quantity} className="flex-1">
                    {isBusy ? 'Salvando...' : 'Salvar alteração'}
                  </Button>
                </div>
              </div>
            ) : (
              /* Item list */
              <div className="space-y-1 max-h-80 overflow-y-auto pr-1">
                {stockItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-md border border-border bg-card px-3 py-2.5 hover:bg-muted/30 transition-colors"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">{item.fabricColorName} — {item.size}</p>
                      <p className="text-xs text-muted-foreground">{item.modelCode} · {item.quantity} un.</p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => { setSuccessMsg(''); setErrorMsg(''); setEditItem(item); setEditQty(String(item.quantity)) }}
                    >
                      <Pencil className="h-3.5 w-3.5 mr-1" />
                      Editar
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* ───── EXCLUIR ───── */}
        {tab === 'excluir' && (
          <div className="space-y-3">
            {itemsLoading ? (
              <p className="text-sm text-muted-foreground text-center py-6">Carregando itens...</p>
            ) : stockItems.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-6">
                Nenhum item em estoque para excluir.
              </p>
            ) : deleteTarget ? (
              /* Confirm delete */
              <div className="space-y-4">
                <div className="rounded-md border border-destructive/30 bg-destructive/10 px-4 py-4 space-y-1.5 text-sm">
                  <p className="flex items-center gap-1.5 font-semibold text-destructive">
                    <AlertTriangle className="h-4 w-4" />
                    Confirmar exclusão
                  </p>
                  <p className="text-muted-foreground">
                    Você está prestes a excluir <strong className="text-foreground">{deleteTarget.fabricColorName} {deleteTarget.size} ({deleteTarget.modelCode})</strong> com <strong className="text-foreground">{deleteTarget.quantity} un.</strong> em estoque.
                  </p>
                  <p className="text-xs text-muted-foreground">
                    Um movimento de saída total será registrado para rastreabilidade. Esta ação não pode ser desfeita.
                  </p>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" onClick={() => setDeleteTarget(null)} disabled={isBusy} className="flex-1">
                    Cancelar
                  </Button>
                  <Button variant="destructive" onClick={handleDelete} disabled={isBusy} className="flex-1">
                    {isBusy ? 'Excluindo...' : 'Confirmar exclusão'}
                  </Button>
                </div>
              </div>
            ) : (
              /* Item list */
              <div className="space-y-1 max-h-80 overflow-y-auto pr-1">
                {stockItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-md border border-border bg-card px-3 py-2.5 hover:bg-muted/30 transition-colors"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">{item.fabricColorName} — {item.size}</p>
                      <p className="text-xs text-muted-foreground">{item.modelCode} · {item.quantity} un.</p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => { setSuccessMsg(''); setErrorMsg(''); setDeleteTarget(item) }}
                      className="text-destructive hover:bg-destructive/10 hover:text-destructive border-destructive/40"
                    >
                      <Trash2 className="h-3.5 w-3.5 mr-1" />
                      Excluir
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
