import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useCreateGenericCategory } from '../hooks/useGenericProductsApi'
import { AlertTriangle } from 'lucide-react'

interface Props {
  open: boolean
  onClose: () => void
}

export function CreateCategoryModal({ open, onClose }: Props) {
  const [name, setName] = useState('')
  const [errorMsg, setErrorMsg] = useState('')
  const createCategory = useCreateGenericCategory()

  function handleClose() {
    setName('')
    setErrorMsg('')
    onClose()
  }

  async function handleCreate() {
    if (!name.trim()) return
    setErrorMsg('')
    try {
      await createCategory.mutateAsync(name)
      handleClose()
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      setErrorMsg(err?.response?.data?.error ?? 'Erro ao criar categoria.')
    }
  }

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Nova Categoria de Produto</DialogTitle>
        </DialogHeader>

        {errorMsg && (
          <div className="flex items-start gap-2 rounded-md border border-destructive/30 bg-destructive/10 px-4 py-2.5 text-sm text-destructive">
            <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
            {errorMsg}
          </div>
        )}

        <div className="space-y-4 pt-2">
          <div>
            <label className="block text-sm font-medium mb-1">Nome da Categoria</label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ex: Bonés, Canecas..."
              autoFocus
            />
          </div>

          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={handleClose} disabled={createCategory.isPending}>
              Cancelar
            </Button>
            <Button onClick={handleCreate} disabled={createCategory.isPending || !name.trim()}>
              {createCategory.isPending ? 'Criando...' : 'Criar categoria'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
