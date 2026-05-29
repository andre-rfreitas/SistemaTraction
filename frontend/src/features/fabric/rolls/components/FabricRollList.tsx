import { useFabricRolls } from '../hooks/useFabricRolls'

const STATUS_LABEL: Record<string, string> = {
  Available: 'Disponível',
  InCutting: 'Em corte',
  Consumed: 'Consumida',
}

const STATUS_COLOR: Record<string, string> = {
  Available: 'bg-green-100 text-green-800',
  InCutting: 'bg-yellow-100 text-yellow-800',
  Consumed: 'bg-neutral-100 text-neutral-600',
}

function fmt(n: number, decimals = 2) {
  return n.toLocaleString('pt-BR', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

export function FabricRollList() {
  const { data: rolls = [], isLoading } = useFabricRolls()

  if (isLoading) return <p className="text-sm text-neutral-500">Carregando...</p>

  if (rolls.length === 0)
    return (
      <p className="text-sm text-neutral-500">
        Nenhuma bobina registrada. Clique em "+ Nova bobina" para começar.
      </p>
    )

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="border-b border-neutral-200 text-left">
            <th className="pb-2 pr-4 font-medium text-neutral-700">Tipo / Variação</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700">Cor</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700 text-right">Peso (kg)</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700 text-right">Preço total</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700 text-right">R$/kg pago</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700 text-right">R$/kg ref.</th>
            <th className="pb-2 pr-4 font-medium text-neutral-700">Recebida em</th>
            <th className="pb-2 font-medium text-neutral-700">Status</th>
          </tr>
        </thead>
        <tbody>
          {rolls.map((r) => {
            const diff = r.pricePerKgActual - r.fabricTypePricePerKg
            return (
              <tr key={r.id} className="border-b border-neutral-100 hover:bg-neutral-50">
                <td className="py-2 pr-4">
                  {r.fabricTypeName} — {r.fabricTypeVariation}
                </td>
                <td className="py-2 pr-4 flex items-center gap-2">
                  {r.fabricColorHexCode && (
                    <span
                      className="inline-block w-3 h-3 rounded-full border border-neutral-300"
                      style={{ backgroundColor: r.fabricColorHexCode }}
                    />
                  )}
                  {r.fabricColorName}
                </td>
                <td className="py-2 pr-4 text-right tabular-nums">{fmt(r.weightKg, 3)}</td>
                <td className="py-2 pr-4 text-right tabular-nums">R$ {fmt(r.priceTotal)}</td>
                <td className="py-2 pr-4 text-right tabular-nums">
                  <span
                    className={
                      diff > 0.001
                        ? 'text-red-600 font-medium'
                        : diff < -0.001
                        ? 'text-green-600 font-medium'
                        : ''
                    }
                  >
                    R$ {fmt(r.pricePerKgActual)}
                  </span>
                </td>
                <td className="py-2 pr-4 text-right tabular-nums text-neutral-500">
                  R$ {fmt(r.fabricTypePricePerKg)}
                </td>
                <td className="py-2 pr-4 text-neutral-600">
                  {new Date(r.receivedAt).toLocaleDateString('pt-BR')}
                </td>
                <td className="py-2">
                  <span
                    className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${
                      STATUS_COLOR[r.status] ?? 'bg-neutral-100 text-neutral-600'
                    }`}
                  >
                    {STATUS_LABEL[r.status] ?? r.status}
                  </span>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
