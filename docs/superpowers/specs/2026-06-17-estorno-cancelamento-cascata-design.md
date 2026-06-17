# Design: Estorno Financeiro com Cancelamento em Cascata

**Data:** 2026-06-17  
**Status:** Aprovado

## Contexto

Quando um lançamento financeiro é estornado na tela financeira, o sistema hoje apenas cria um lançamento de valor oposto — não cancela o pedido vinculado nem reverte os efeitos colaterais (estoque, status de bobinas, etc.).

O objetivo é fazer o estorno propagar automaticamente o cancelamento para o pedido de origem e reverter todos os efeitos que ele causou.

## Escopo

Tratado neste design:
- `ReferenceType == "CuttingDelivery"` — entrega do corte
- `ReferenceType == "SewingDelivery"` — entrega da costura

Fora do escopo (sem cascata, comportamento atual mantido):
- `ReferenceType == "FabricRoll"`
- `ReferenceType == "ShopifyOrder"` (já bloqueado na UI)
- Lançamentos manuais sem referência

## Decisões

| Questão | Decisão |
|---|---|
| Quais ReferenceTypes causam cascata? | CuttingDelivery + SewingDelivery |
| Estornar Costura estorna Defeitos automaticamente? | Sim — todos os lançamentos do mesmo SewingDelivery são estornados juntos |
| Estornar Corte quando já há costura? | Bloquear — usuário deve estornar a costura primeiro |
| Pedido cancelado aparece na lista? | Sim, com badge "Cancelado" |
| FabricRolls consumidas voltam para qual status? | Available |

## Mudanças de Domínio

### `CuttingOrderStatus` — novo valor `Cancelled`

```csharp
public enum CuttingOrderStatus
{
    Draft, SentToCutter, Delivered, SewingDelivered, Cancelled
}
```

### `CuttingOrder` — novo método `CancelDelivered()`

O método `Cancel()` existente (usado no cancelamento manual) permanece com sua restrição atual.  
`CancelDelivered()` é chamado exclusivamente pelo estorno financeiro e permite cancelar pedidos em qualquer status de entrega:

```csharp
public void CancelDelivered()
{
    Status = CuttingOrderStatus.Cancelled;
    TouchUpdatedAt();
}
```

O `Cancel()` existente também precisa ser atualizado para usar `Status = Cancelled` em vez de `MarkAsDeleted()`, mas mantendo sua validação de bloquear pedidos Delivered/SewingDelivered.

### `FabricRoll.RevertToAvailable()` — aceitar status `Consumed`

```csharp
public void RevertToAvailable()
{
    if (Status != FabricRollStatus.InCutting && Status != FabricRollStatus.Consumed)
        throw new DomainException("Bobina não pode ser revertida para disponível.");
    Status = FabricRollStatus.Available;
    TouchUpdatedAt();
}
```

## Mudanças na Application Layer

### `ReverseFinancialEntryCommandHandler` — lógica de cascata

O handler detecta o `ReferenceType` do lançamento original e executa efeitos adicionais dentro da mesma transação (atomicidade garantida).

#### Fluxo — `CuttingDelivery`

1. Busca `CuttingDelivery` pelo `original.ReferenceId`
2. Busca `CuttingOrder` com `Items → FabricRoll` incluídos
3. Verifica se existe `SewingDelivery` vinculada — se sim, lança `DomainException`: _"Estorno bloqueado: já existe entrega de costura para este pedido. Estorne a costura primeiro."_
4. Chama `roll.RevertToAvailable()` para cada FabricRoll dos itens do pedido
5. Chama `order.CancelDelivered()`
6. Cria `FinancialEntry.CreateReversal(original)` normalmente

#### Fluxo — `SewingDelivery`

1. Busca `SewingDelivery` pelo `original.ReferenceId`
2. Busca o `CuttingOrder` via `sewingDelivery.CuttingOrderId`
3. Busca **todos** os lançamentos financeiros com `ReferenceId == sewingDelivery.Id` que ainda não foram estornados
4. Para cada lançamento: cria `FinancialEntry.CreateReversal(entry)`
5. Busca os `ShirtStockMovements` com `SourceOrderId == cuttingOrder.Id`
6. Para cada movimento: decrementa `StockItem.RemoveStock(qty)` e cria um `ShirtStockMovement` compensatório negativo com razão `"Estorno costura pedido #{orderNumber}"`
7. Chama `order.CancelDelivered()`

#### Lançamentos sem cascata

Lançamentos com `ReferenceType` não reconhecido ou nulo: apenas cria o estorno financeiro (comportamento atual).

## Mudanças de Infraestrutura / Banco

- Nova migration: adicionar `Cancelled` ao enum `CuttingOrderStatus` (valor inteiro 4)
- `StockItem` precisa de método `RemoveStock(int qty)` que lança `DomainException` se o resultado for negativo

## Mudanças no Frontend

### `CuttingOrderDto.status` — novo valor

```ts
status: 'Draft' | 'SentToCutter' | 'Delivered' | 'SewingDelivered' | 'Cancelled'
```

### `CuttingOrderList` — badge para Cancelado

Adicionar tratamento visual para status `Cancelled` (badge vermelho/neutro), sem ações disponíveis (sem botões de editar, enviar, registrar entrega).

### `EntriesTable` — aviso contextual no dialog de confirmação

- `referenceType === 'CuttingDelivery'`: aviso _"Este estorno também cancelará o pedido de corte vinculado e reverterá as bobinas de tecido para disponível."_
- `referenceType === 'SewingDelivery'`: aviso _"Este estorno também cancelará o pedido de corte, reverterá o estoque de camisetas e estornará todos os lançamentos vinculados (Costura e Defeitos)."_

### `useReverseFinancialEntry` — invalidar queries adicionais

```ts
onSuccess: () => {
  queryClient.invalidateQueries({ queryKey: ['financial-summary'] })
  queryClient.invalidateQueries({ queryKey: ['financial-entries'] })
  queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
  queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
  queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
}
```

## Fluxo de Erro e Casos Limite

| Situação | Comportamento |
|---|---|
| Estornar CuttingDelivery com SewingDelivery existente | 400 DomainException — mensagem orientativa |
| Estornar SewingDelivery quando StockItem ficaria negativo | 400 DomainException — estoque insuficiente |
| Lançamento já estornado | 400 DomainException — comportamento atual mantido |
| SewingDelivery com Defeitos = 0 (sem lançamento de Defeitos) | Sistema estorna apenas o lançamento existente, sem erro |

## Fora do Escopo Deste Design

- Reverter efeitos do `FabricRoll` (Tecido) ao estornar
- Reverter insumos (`SupplyStock`) consumidos durante a separação
- Reverter DTF stock ao cancelar pedido DTF via financeiro
