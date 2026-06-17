# Design: Cancelamento Manual de Pedidos de Corte com Estorno Automático

**Data:** 2026-06-17  
**Status:** Aprovado

## Contexto

Atualmente o botão "Cancelar" nos pedidos de corte só aparece para pedidos em status `Draft` ou `SentToCutter`. Pedidos já entregues (`Delivered` ou `SewingDelivered`) só podem ser cancelados via estorno financeiro na tela financeira.

O objetivo é permitir que o usuário cancele qualquer pedido de corte diretamente da tela de pedidos, e quando o pedido já tiver lançamentos financeiros vinculados, o cancelamento dispare automaticamente o estorno financeiro em cascata.

## Decisões

| Questão | Decisão |
|---|---|
| Como o backend encontra o lançamento financeiro a estornar? | Handler busca `FinancialEntry` via `ReferenceType` + `ReferenceId` da `CuttingDelivery` ou `SewingDelivery` |
| O handler de cancelamento duplica a lógica de cascata? | Não — despacha `ReverseFinancialEntryCommand` via MediatR, reutilizando o handler existente |
| O que acontece se não houver lançamento financeiro? | Chama `order.CancelDelivered()` diretamente e salva |
| O contrato do endpoint muda? | Não — `DELETE /cutting-orders/{id}` continua retornando `Unit` |
| O `Cancel()` do domínio muda? | Não — continua bloqueando Delivered/SewingDelivered. O handler chama `CancelDelivered()` nos casos com lançamento, e o cascade do estorno também chama `CancelDelivered()` |

## Mudanças no Backend

### `CancelCuttingOrderCommandHandler`

Fluxo atualizado:

```
Draft         → cancel simples (comportamento atual, sem alteração)
SentToCutter  → reverter bobinas + Cancel() (comportamento atual, sem alteração)
Delivered     → buscar CuttingDelivery vinculada ao pedido
                → buscar FinancialEntry com ReferenceType="CuttingDelivery", ReferenceId=delivery.Id
                → se achou: mediator.Send(new ReverseFinancialEntryCommand(entryId))
                  [o cascade já chama CancelDelivered() + SaveChangesAsync — handler não salva novamente]
                → se não achou: order.CancelDelivered() + SaveChangesAsync
SewingDelivered → buscar SewingDelivery vinculada ao pedido
                  → buscar FinancialEntry com ReferenceType="SewingDelivery", ReferenceId=delivery.Id
                  → se achou: mediator.Send(new ReverseFinancialEntryCommand(entryId))
                  → se não achou: order.CancelDelivered() + SaveChangesAsync
```

O handler precisa receber `ISender` (MediatR) além do `IApplicationDbContext`.

### Sem novas migrations

Nenhuma mudança de schema — apenas lógica de application layer.

## Mudanças no Frontend

### `CuttingOrderList.tsx`

```ts
// antes
const canCancel = !isCancelled && (o.status === 'Draft' || o.status === 'SentToCutter')

// depois
const canCancel = !isCancelled
```

### `CuttingOrderPage.tsx` — dialog de confirmação

Texto do dialog varia por status:

| Status | Mensagem |
|---|---|
| `Draft` | "O rascunho será removido permanentemente." |
| `SentToCutter` | "O pedido foi enviado ao cortador. As bobinas serão revertidas para disponível." |
| `Delivered` | "Este cancelamento estornará o lançamento financeiro de corte e reverterá as bobinas de tecido para disponível." |
| `SewingDelivered` | "Este cancelamento estornará os lançamentos de costura e defeitos, reverterá o estoque de camisetas e cancelará o pedido." |

### `useCancelCuttingOrder.ts` — invalidações adicionais

```ts
onSuccess: () => {
  queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
  queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
  queryClient.invalidateQueries({ queryKey: ['financial-entries'] })   // novo
  queryClient.invalidateQueries({ queryKey: ['financial-summary'] })   // novo
  queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })         // novo
}
```

## Fluxo de Erro e Casos Limite

| Situação | Comportamento |
|---|---|
| Cancelar `Delivered` com SewingDelivery existente | Bloqueado pelo `ReverseFinancialEntryCommandHandler` existente: 400 "Estorne a costura primeiro." |
| Cancelar pedido já cancelado | Botão não aparece (isCancelled = true) |
| Cancelar `Delivered` sem lançamento financeiro | `CancelDelivered()` direto, sem estorno |
| Cancelar `SewingDelivered` e estoque ficaria negativo | 400 DomainException do handler de estorno existente |

## Fora do Escopo

- Cancelamento de pedidos DTF
- Cancelamento via tela financeira (comportamento existente mantido sem alteração)
