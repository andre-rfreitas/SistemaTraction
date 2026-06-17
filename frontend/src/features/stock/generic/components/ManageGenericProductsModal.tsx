import { useState } from 'react'
import { Trash2, Pencil, Plus, AlertTriangle, Package } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { 
  useGenericProducts, 
  useCreateGenericProduct, 
  useAdjustGenericProductStock, 
  useDeleteGenericProduct,
  type GenericProduct 
} from '../hooks/useGenericProductsApi'
import { cn } from '@/lib/utils'

type Tab = 'cadastrar' | 'editar' | 'excluir'

interface Props {
  open: boolean
  onClose: () => void
  categoryId: string
  categoryName: string
}

export function ManageGenericProductsModal({ open, onClose, categoryId, categoryName }: Props) {
  const [tab, setTab] = useState<Tab>('cadastrar')
  
  // Cadastrar state
  const [newName, setNewName] = useState('')
  const [newQuantity, setNewQuantity] = useState('')
  const [newUnitCost, setNewUnitCost] = useState('')
  const [newReason, setNewReason] = useState('Cadastro manual')
  const [confirmStep, setConfirmStep] = useState(false)

  // Editar state
  const [editItem, setEditItem] = useState<GenericProduct | null>(null)
  const [editQty, setEditQty] = useState('')
  const [editUnitCost, setEditUnitCost] = useState('')

  // Excluir state
  const [deleteTarget, setDeleteTarget] = useState<GenericProduct | null>(null)

  // Feedback state
  const [errorMsg, setErrorMsg] = useState('')
  const [successMsg, setSuccessMsg] = useState('')

  const { data: stockItems = [], isLoading: itemsLoading } = useGenericProducts(categoryId)
  const createProduct = useCreateGenericProduct()
  const adjustProduct = useAdjustGenericProductStock()
  const deleteProduct = useDeleteGenericProduct()

  const isBusy = createProduct.isPending || adjustProduct.isPending || deleteProduct.isPending

  function resetAll() {
    setNewName('')
    setNewQuantity('')
    setNewUnitCost('')
    setNewReason('Cadastro manual')
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

  const qty = parseInt(newQuantity) || 0
  const unitCost = parseFloat(newUnitCost.replace(',', '.')) || 0
  const totalCost = qty * unitCost
  const isCadastrarValid = newName.trim() && qty > 0 && newReason.trim()

  async function handleCadastrar() {
    if (!isCadastrarValid) return
    setErrorMsg('')
    try {
      await createProduct.mutateAsync({
        categoryId,
        name: newName,
        initialQuantity: qty,
        unitCost: unitCost > 0 ? unitCost : undefined,
        reason: newReason,
      })
      setSuccessMsg(`✓ ${qty} un. de ${newName} cadastradas com sucesso!`)
      setNewName('')
      setNewQuantity('')
      setNewUnitCost('')
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
      await adjustProduct.mutateAsync({
        productId: editItem.id,
        categoryId,
        adjustmentType: delta > 0 ? 'Entrada' : 'Saída',
        quantity: Math.abs(delta),
        reason: 'Edição manual de estoque',
        unitCost: delta > 0 && uc > 0 ? uc : undefined,
      })
      setSuccessMsg(`✓ ${editItem.name} atualizado para ${newQty} un.`)
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
      await deleteProduct.mutateAsync({ productId: deleteTarget.id, categoryId })
      setSuccessMsg(`✓ ${deleteTarget.name} removido do estoque.`)
      setDeleteTarget(null)
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao excluir item.')
    }
  }

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
            Gerenciar {categoryName}
          </DialogTitle>
        </DialogHeader>

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
              <div className="space-y-4">
                <div className="rounded-md border border-border bg-muted/50 p-4 space-y-2 text-sm">
                  <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Resumo do cadastro</p>
                  <div className="grid grid-cols-2 gap-y-1.5">
                    <span className="text-muted-foreground">Produto:</span>
                    <span className="font-medium">{newName}</span>
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
                    <span>{newReason}</span>
                  </div>
                </div>
                {unitCost > 0 && (
                  <p className="text-xs text-muted-foreground">
                    Uma despesa de <strong>R$ {totalCost.toFixed(2)}</strong> será lançada no financeiro (categoria Estoque).
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
              <>
                <div>
                  <label className="block text-sm font-medium mb-1">Nome do Produto</label>
                  <Input
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                    placeholder={`Ex: ${categoryName} Preto, ${categoryName} Branco...`}
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium mb-1">Quantidade</label>
                    <Input
                      type="number"
                      min="1"
                      value={newQuantity}
                      onChange={(e) => setNewQuantity(e.target.value)}
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
                      value={newUnitCost}
                      onChange={(e) => setNewUnitCost(e.target.value)}
                      placeholder="0,00"
                    />
                  </div>
                </div>

                {unitCost > 0 && qty > 0 && (
                  <div className="rounded-md bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                    Total a lançar no financeiro:{' '}
                    <span className="font-semibold text-destructive">R$ {totalCost.toFixed(2)}</span>
                  </div>
                )}

                <div>
                  <label className="block text-sm font-medium mb-1">Motivo</label>
                  <Input
                    value={newReason}
                    onChange={(e) => setNewReason(e.target.value)}
                    placeholder="Ex: produção, compra..."
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
                Nenhum produto cadastrado para editar.
              </p>
            ) : editItem ? (
              <div className="space-y-4">
                <div className="rounded-md border border-primary/30 bg-primary/5 px-4 py-3 text-sm">
                  <p className="font-medium text-foreground">{editItem.name}</p>
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
              <div className="space-y-1 max-h-80 overflow-y-auto pr-1">
                {stockItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-md border border-border bg-card px-3 py-2.5 hover:bg-muted/30 transition-colors"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">{item.name}</p>
                      <p className="text-xs text-muted-foreground">{item.quantity} un.</p>
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
                Nenhum produto para excluir.
              </p>
            ) : deleteTarget ? (
              <div className="space-y-4">
                <div className="rounded-md border border-destructive/30 bg-destructive/10 px-4 py-4 space-y-1.5 text-sm">
                  <p className="flex items-center gap-1.5 font-semibold text-destructive">
                    <AlertTriangle className="h-4 w-4" />
                    Confirmar exclusão
                  </p>
                  <p className="text-muted-foreground">
                    Você está prestes a excluir <strong className="text-foreground">{deleteTarget.name}</strong> com <strong className="text-foreground">{deleteTarget.quantity} un.</strong> em estoque.
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
              <div className="space-y-1 max-h-80 overflow-y-auto pr-1">
                {stockItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-md border border-border bg-card px-3 py-2.5 hover:bg-muted/30 transition-colors"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">{item.name}</p>
                      <p className="text-xs text-muted-foreground">{item.quantity} un.</p>
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
