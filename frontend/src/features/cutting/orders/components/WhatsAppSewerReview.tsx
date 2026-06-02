import { useState } from 'react'
import type { RegisterCuttingDeliveryResult } from '../types'
import { Button } from '@/components/ui/button'

interface Props {
  result: RegisterCuttingDeliveryResult
  onDone: () => void
}

export function WhatsAppSewerReview({ result, onDone }: Props) {
  const [message, setMessage] = useState(result.whatsAppMessage)
  const [copied, setCopied] = useState(false)

  async function handleCopy() {
    await navigator.clipboard.writeText(message)
    setCopied(true)
    setTimeout(() => setCopied(false), 2500)
  }

  const waMeLink = result.sewerPhone
    ? `https://wa.me/${result.sewerPhone.replace(/\D/g, '')}?text=${encodeURIComponent(message)}`
    : null

  return (
    <div className="space-y-4">
      {/* Resumo financeiro */}
      <div className="rounded-md bg-success/10 border border-success/20 p-3 text-sm space-y-1">
        <p className="font-semibold text-success">Entrega registrada com sucesso!</p>
        <div className="flex justify-between text-success">
          <span>{result.totalPieces} peças entregues</span>
          <span className="font-medium">
            Custo corte: R$ {result.cuttingCostTotal.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
          </span>
        </div>
      </div>

      {/* Destinatário */}
      <div className="flex items-center gap-3 p-3 bg-success/10 border border-success/20 rounded-md">
        <div className="w-9 h-9 rounded-full bg-success flex items-center justify-center text-white font-bold text-base shrink-0">
          {result.sewerName.charAt(0).toUpperCase()}
        </div>
        <div>
          <p className="text-sm font-semibold text-foreground">{result.sewerName}</p>
          <p className="text-xs text-muted-foreground">
            {result.sewerPhone || <span className="italic">Número não configurado</span>}
          </p>
        </div>
      </div>

      {/* Mensagem */}
      <div>
        <div className="flex items-center justify-between mb-1">
          <label className="text-sm font-medium text-foreground">Mensagem para o costureiro</label>
          <button
            onClick={handleCopy}
            className={`text-xs px-2 py-1 rounded transition-colors font-medium ${
              copied
                ? 'bg-success/10 text-success'
                : 'bg-muted text-muted-foreground hover:bg-muted/80'
            }`}
          >
            {copied ? '✓ Copiado!' : 'Copiar'}
          </button>
        </div>
        <textarea
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          rows={8}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground font-mono resize-none shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        />
        <p className="text-xs text-muted-foreground mt-1">Você pode editar antes de enviar.</p>
      </div>

      {/* Ações */}
      <div className="flex flex-col gap-2">
        {waMeLink ? (
          <a
            href={waMeLink}
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
        ) : (
          <p className="text-xs text-warning bg-warning/10 border border-warning/20 rounded-md px-3 py-2">
            Configure o número do costureiro em Configurações para abrir diretamente no WhatsApp.
          </p>
        )}
        <Button onClick={onDone} variant="outline" className="w-full">Fechar</Button>
      </div>
    </div>
  )
}
