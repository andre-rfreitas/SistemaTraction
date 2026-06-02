import { useDtfStockDetail } from '../hooks/useDtfStockDetail'
import { MOVEMENT_TYPE_LABEL, MOVEMENT_TYPE_CLASS, type DtfMovementType } from '../types'

interface Props {
  dtfModelId: string
}

export function DtfStockDetail({ dtfModelId }: Props) {
  const { data, isLoading } = useDtfStockDetail(dtfModelId)

  if (isLoading)
    return <p className="text-sm text-muted-foreground">Carregando histórico...</p>

  if (!data || data.movements.length === 0)
    return (
      <p className="text-sm text-muted-foreground italic">
        Nenhuma movimentação registrada ainda.
      </p>
    )

  return (
    <div className="space-y-1">
      {data.movements.map((m) => {
        const label = MOVEMENT_TYPE_LABEL[m.type as DtfMovementType]
        const cls = MOVEMENT_TYPE_CLASS[m.type as DtfMovementType]
        const sign = m.delta > 0 ? '+' : ''
        const date = new Date(m.createdAt).toLocaleString('pt-BR', {
          day: '2-digit',
          month: '2-digit',
          hour: '2-digit',
          minute: '2-digit',
        })

        return (
          <div
            key={m.id}
            className="flex items-center justify-between py-1.5 border-b border-border last:border-0 text-sm"
          >
            <div className="flex items-center gap-3">
              <span className={`font-medium w-14 ${cls}`}>{label}</span>
              <span className={`font-mono font-semibold ${cls}`}>
                {sign}{m.delta}
              </span>
              {m.reason && (
                <span className="text-muted-foreground truncate max-w-[160px]">
                  {m.reason}
                </span>
              )}
            </div>
            <span className="text-muted-foreground shrink-0">{date}</span>
          </div>
        )
      })}
    </div>
  )
}
