import { useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useUpsertSkuCode } from '@/features/separation/hooks/useSkuCodes'
import { AlertTriangle } from 'lucide-react'

interface Props {
  open: boolean
  onClose: () => void
}

export function CreateModelModal({ open, onClose }: Props) {
  const [code, setCode] = useState('')
  const [value, setValue] = useState('')
  const [errorMsg, setErrorMsg] = useState('')

  const upsert = useUpsertSkuCode()

  function handleClose() {
    setCode('')
    setValue('')
    setErrorMsg('')
    onClose()
  }

  async function handleSave() {
    if (!code.trim() || !value.trim()) {
      setErrorMsg('Preencha o código e a descrição.')
      return
    }

    try {
      await upsert.mutateAsync({
        code: code.trim().toUpperCase(),
        value: value.trim(),
        category: 'Modelo',
      })
      handleClose()
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao cadastrar modelo.')
    }
  }

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Cadastrar Novo Modelo</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {errorMsg && (
            <div className="flex items-start gap-2 rounded-md border border-destructive/30 bg-destructive/10 px-4 py-2 text-sm text-destructive">
              <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
              {errorMsg}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium mb-1">
              Código (Sigla) <span className="text-destructive">*</span>
            </label>
            <Input
              value={code}
              onChange={(e) => setCode(e.target.value.toUpperCase())}
              placeholder="Ex: BBL"
              maxLength={10}
              autoFocus
            />
            <p className="mt-1 text-xs text-muted-foreground">O código que aparece no arquivo PDF (antes do primeiro traço).</p>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              Descrição do Modelo <span className="text-destructive">*</span>
            </label>
            <Input
              value={value}
              onChange={(e) => setValue(e.target.value)}
              placeholder="Ex: Babylook"
              maxLength={50}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={upsert.isPending}>
            Cancelar
          </Button>
          <Button onClick={handleSave} disabled={upsert.isPending || !code.trim() || !value.trim()}>
            {upsert.isPending ? 'Salvando...' : 'Salvar Modelo'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
