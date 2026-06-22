import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useFabricTypes } from '@/features/settings/fabric/hooks/useFabricTypes'
import type { AdjustPayload } from '../hooks/useAdjustShirtStock'


const TYPES = ['Entrada', 'Saída'] as const
type AdjType = typeof TYPES[number]

interface Props {
  sizes: string[]
  modelCode: string
  isLoading: boolean
  onConfirm: (payload: AdjustPayload) => void
  onClose: () => void
}

interface FormState {
  fabricColorId: string
  size: string
  adjustmentType: AdjType
  quantity: string
  reason: string
}

const EMPTY: FormState = { fabricColorId: '', size: '', adjustmentType: 'Entrada', quantity: '', reason: '' }

export function StockAdjustmentModal({ sizes, modelCode, isLoading, onConfirm, onClose }: Props) {
  const { data: fabricTypes = [] } = useFabricTypes()
  const [form, setForm] = useState<FormState>(EMPTY)
  const [step, setStep] = useState<'form' | 'confirm'>('form')

  const allColors = fabricTypes.flatMap((t) =>
    t.colors.map((c) => ({ id: c.id, label: `${c.name} — ${t.name} ${t.variation}`, name: c.name }))
  )

  const set = (key: keyof FormState, value: string) =>
    setForm((f) => ({ ...f, [key]: value }))

  const qty = parseInt(form.quantity) || 0
  const isValid = form.fabricColorId && form.size && qty > 0 && form.reason.trim()
  const selectedColor = allColors.find((c) => c.id === form.fabricColorId)

  if (step === 'confirm') {
    return (
      <div className="space-y-4">
        <div className="rounded-md border border-border bg-muted/50 p-4 space-y-2 text-sm">
          <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">Resumo do ajuste</p>
          <div className="grid grid-cols-2 gap-1">
            <span className="text-muted-foreground">Cor:</span>
            <span className="font-medium">{selectedColor?.name}</span>
            <span className="text-muted-foreground">Tamanho:</span>
            <span className="font-medium">{form.size}</span>
            <span className="text-muted-foreground">Tipo:</span>
            <span className={`font-medium ${form.adjustmentType === 'Entrada' ? 'text-success' : 'text-destructive'}`}>
              {form.adjustmentType}
            </span>
            <span className="text-muted-foreground">Quantidade:</span>
            <span className="font-bold">{qty}</span>
            <span className="text-muted-foreground">Motivo:</span>
            <span>{form.reason}</span>
          </div>
        </div>
        <p className="text-xs text-muted-foreground">Esta operação atualizará o estoque imediatamente.</p>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setStep('form')} disabled={isLoading} className="flex-1">Editar</Button>
          <Button
            onClick={() => onConfirm({ fabricColorId: form.fabricColorId, size: form.size, adjustmentType: form.adjustmentType, quantity: qty, reason: form.reason, modelCode })}
            disabled={isLoading}
            className="flex-1"
          >
            {isLoading ? 'Aplicando...' : 'Confirmar'}
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium mb-1">Cor</label>
        <select
          value={form.fabricColorId}
          onChange={(e) => set('fabricColorId', e.target.value)}
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
            onChange={(e) => set('size', e.target.value)}
            className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <option value="">Selecione...</option>
            {sizes.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Tipo</label>
          <div className="flex gap-1 h-9">
            {TYPES.map((t) => (
              <button
                key={t}
                onClick={() => set('adjustmentType', t)}
                className={`flex-1 rounded-md border text-sm font-medium transition-colors ${
                  form.adjustmentType === t
                    ? t === 'Entrada' ? 'border-success bg-success/10 text-success' : 'border-destructive bg-destructive/10 text-destructive'
                    : 'border-border text-muted-foreground hover:border-primary/40'
                }`}
              >{t}</button>
            ))}
          </div>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Quantidade</label>
        <Input
          type="number"
          min="1"
          value={form.quantity}
          onChange={(e) => set('quantity', e.target.value)}
          placeholder="0"
          className="max-w-[120px]"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Motivo</label>
        <Input
          value={form.reason}
          onChange={(e) => set('reason', e.target.value)}
          placeholder="Ex: ajuste de contagem, devolução..."
        />
      </div>

      <div className="flex gap-2">
        <Button variant="outline" onClick={onClose} className="flex-1">Cancelar</Button>
        <Button onClick={() => setStep('confirm')} disabled={!isValid} className="flex-1">Revisar →</Button>
      </div>
    </div>
  )
}
