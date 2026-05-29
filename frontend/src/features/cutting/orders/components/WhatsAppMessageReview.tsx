import { useState } from 'react'
import type { CreateCuttingOrderResult } from '../types'
import { Button } from '@/components/ui/button'

interface Props {
  result: CreateCuttingOrderResult
  isSending: boolean
  onConfirmSend: () => void
  onDone: () => void
}

export function WhatsAppMessageReview({ result, isSending, onConfirmSend, onDone }: Props) {
  const [message, setMessage] = useState(result.whatsAppMessage)
  const [copied, setCopied] = useState(false)
  const [sent, setSent] = useState(false)

  async function handleCopy() {
    await navigator.clipboard.writeText(message)
    setCopied(true)
    setTimeout(() => setCopied(false), 2500)
  }

  function handleConfirm() {
    onConfirmSend()
    setSent(true)
  }

  const waMeLink = result.cutterPhone
    ? `https://wa.me/${result.cutterPhone.replace(/\D/g, '')}?text=${encodeURIComponent(message)}`
    : null

  return (
    <div className="space-y-4">
      {/* Recipient */}
      <div className="flex items-center gap-3 p-3 bg-green-50 border border-green-200 rounded-md">
        <div className="w-9 h-9 rounded-full bg-green-500 flex items-center justify-center text-white font-bold text-base shrink-0">
          {result.cutterName.charAt(0).toUpperCase()}
        </div>
        <div>
          <p className="text-sm font-semibold text-green-900">{result.cutterName}</p>
          <p className="text-xs text-green-700">
            {result.cutterPhone || <span className="italic">Número não configurado</span>}
          </p>
        </div>
      </div>

      {/* Message box */}
      <div>
        <div className="flex items-center justify-between mb-1">
          <label className="text-sm font-medium text-neutral-700">Mensagem</label>
          <button
            onClick={handleCopy}
            className={`text-xs px-2 py-1 rounded transition-colors font-medium ${
              copied
                ? 'bg-green-100 text-green-700'
                : 'bg-neutral-100 text-neutral-600 hover:bg-neutral-200'
            }`}
          >
            {copied ? '✓ Copiado!' : 'Copiar'}
          </button>
        </div>
        <textarea
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          rows={7}
          className="w-full border border-neutral-300 rounded-md px-3 py-2 text-sm font-mono resize-none focus:outline-none focus:ring-2 focus:ring-neutral-400"
        />
        <p className="text-xs text-neutral-400 mt-1">Você pode editar a mensagem antes de enviar.</p>
      </div>

      {/* Actions */}
      {sent ? (
        <div className="space-y-2">
          <p className="text-sm text-green-700 bg-green-50 border border-green-200 rounded-md px-3 py-2">
            Pedido #{result.orderNumber} marcado como enviado. A bobina está em corte.
          </p>
          <Button onClick={onDone} className="w-full" variant="outline">
            Fechar
          </Button>
        </div>
      ) : (
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
            <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-md px-3 py-2">
              Configure o número do cortador em Configurações para abrir diretamente no WhatsApp.
            </p>
          )}
          <Button
            onClick={handleConfirm}
            disabled={isSending}
            className="w-full"
          >
            {isSending ? 'Marcando...' : 'Confirmar envio'}
          </Button>
        </div>
      )}
    </div>
  )
}
