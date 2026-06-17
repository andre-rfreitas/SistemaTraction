import { useState } from 'react'
import { Package, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { 
  useGenericProducts, 
  useDeleteGenericCategory 
} from '../hooks/useGenericProductsApi'
import { GenericProductMovementsTable } from './GenericProductMovementsTable'
import { ManageGenericProductsModal } from './ManageGenericProductsModal'

interface Props {
  categoryId: string
  categoryName: string
  onCategoryDeleted: () => void
}

export function GenericProductCategoryView({ categoryId, categoryName, onCategoryDeleted }: Props) {
  const [isManageOpen, setIsManageOpen] = useState(false)
  const { data: products, isLoading } = useGenericProducts(categoryId)
  const deleteCategory = useDeleteGenericCategory()

  async function handleDeleteCategory() {
    if (products && products.length > 0) {
      alert('Não é possível excluir uma categoria que possui produtos. Exclua os produtos primeiro.')
      return
    }
    if (confirm(`Tem certeza que deseja excluir a categoria "${categoryName}"?`)) {
      await deleteCategory.mutateAsync(categoryId)
      onCategoryDeleted()
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => setIsManageOpen(true)}
            className="gap-1.5"
          >
            <Package className="h-4 w-4" />
            Gerenciar produtos
          </Button>
        </div>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:bg-destructive/10 hover:text-destructive"
          onClick={handleDeleteCategory}
          disabled={deleteCategory.isPending || (products && products.length > 0)}
          title={products && products.length > 0 ? "Exclua todos os produtos antes de excluir a categoria" : "Excluir categoria"}
        >
          <Trash2 className="h-4 w-4 mr-2" />
          Excluir Categoria
        </Button>
      </div>

      <div className="space-y-4">
        <h2 className="text-sm font-semibold text-foreground">Produtos em Estoque</h2>
        {isLoading ? (
          <Skeleton className="h-32 w-full rounded-lg" />
        ) : products?.length === 0 ? (
          <div className="rounded-lg border border-border bg-card p-8 text-center text-sm text-muted-foreground">
            Nenhum produto cadastrado na categoria <span className="font-medium text-foreground">{categoryName}</span>.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {products?.map(p => (
              <div key={p.id} className="rounded-lg border border-border bg-card p-4 flex flex-col gap-1 shadow-sm">
                <span className="font-medium text-foreground">{p.name}</span>
                <span className="text-2xl font-bold text-primary">{p.quantity} <span className="text-sm font-normal text-muted-foreground">un.</span></span>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="space-y-3 pt-4 border-t border-border">
        <h2 className="text-sm font-semibold text-foreground">Últimas Movimentações</h2>
        <div className="grid grid-cols-1 gap-4">
          {products?.map(p => (
            <div key={p.id} className="space-y-2">
              <h3 className="text-xs font-medium text-muted-foreground uppercase tracking-wider">{p.name}</h3>
              <GenericProductMovementsTable productId={p.id} />
            </div>
          ))}
          {products?.length === 0 && (
            <p className="text-sm text-muted-foreground">Sem histórico para exibir.</p>
          )}
        </div>
      </div>

      <ManageGenericProductsModal
        open={isManageOpen}
        onClose={() => setIsManageOpen(false)}
        categoryId={categoryId}
        categoryName={categoryName}
      />
    </div>
  )
}
