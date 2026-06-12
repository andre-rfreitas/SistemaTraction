# Pedidos DTF — Design Spec

**Data:** 2026-06-12  
**Status:** Aprovado

## Contexto

O sistema já possui `DtfModel` (modelos de estampa com fator de conversão folha→estampa) e `DtfStockItem` (estoque em estampas por modelo). Hoje, entradas de estoque são registradas manualmente na tela "Estoque DTF". O módulo **Pedidos DTF** formaliza o processo de compra de folhas junto ao fornecedor externo: o usuário cria um pedido, marca como enviado, e ao marcar como recebido o estoque é atualizado automaticamente.

Criação é **exclusivamente manual** — não há geração automática a partir da Lista de Separação.

---

## Modelo de Dados

### `DtfOrderStatus` (enum)

```csharp
public enum DtfOrderStatus { Draft, Sent, Received }
```

### `DtfOrder` (aggregate root)

| Campo | Tipo C# | Coluna SQL | Notas |
|---|---|---|---|
| `Id` | Guid | PK | |
| `OrderNumber` | int | int NOT NULL | Auto-incrementado sequencialmente |
| `Status` | DtfOrderStatus | nvarchar(20) | Persistido como string |
| `Notes` | string? | nvarchar(500) NULL | |
| `SentAt` | DateTime? | datetime2 NULL | Preenchido em `MarkSent()` |
| `ReceivedAt` | DateTime? | datetime2 NULL | Preenchido em `MarkReceived()` |
| `Items` | `IReadOnlyCollection<DtfOrderItem>` | — | Navegação |
| `CreatedAt` | DateTime | datetime2 NOT NULL | BaseEntity |
| `UpdatedAt` | DateTime | datetime2 NOT NULL | BaseEntity |
| `IsDeleted` | bool | bit NOT NULL | Soft delete |

**Métodos de domínio:**
- `Create(notes?)` — factory; Status = Draft
- `AddItem(dtfModelId, sheetQuantity)` — valida sheetQuantity > 0; não permite duplicatas de DtfModelId
- `RemoveItem(dtfModelId)` — só em Draft
- `UpdateNotes(notes?)` — só em Draft
- `MarkSent()` — Draft → Sent; exige ao menos 1 item; define SentAt
- `MarkReceived()` — Sent → Received; define ReceivedAt
- `Cancel()` — só em Draft ou Sent; chama `MarkAsDeleted()`

**Regras:**
- `OrderNumber` gerado via `MAX(OrderNumber) + 1` no handler de criação (dentro da transação); se não houver pedidos ainda, começa em 1
- Received não pode ser cancelado
- Itens não podem ser alterados após status != Draft

### `DtfOrderItem`

| Campo | Tipo C# | Coluna SQL | Notas |
|---|---|---|---|
| `Id` | Guid | PK | |
| `DtfOrderId` | Guid | FK DtfOrders | Cascade delete |
| `DtfModelId` | Guid | FK DtfModels | SetNull on delete |
| `SheetQuantity` | int | int NOT NULL | > 0 |
| `CreatedAt` | DateTime | — | BaseEntity |
| `UpdatedAt` | DateTime | — | BaseEntity |
| `IsDeleted` | bool | — | BaseEntity |

---

## Backend

### Domain (`Domain/Dtf/`)

**Criar:**
- `DtfOrderStatus.cs` — enum
- `DtfOrder.cs` — aggregate com todos os métodos acima
- `DtfOrderItem.cs` — entidade sem métodos próprios

### Application (`Application/Dtf/`)

**DTOs:**
```csharp
public record DtfOrderItemDto(
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int SheetQuantity,
    int StampsPerSheet,
    int StampsTotal);  // SheetQuantity * StampsPerSheet

public record DtfOrderDto(
    Guid Id,
    int OrderNumber,
    DtfOrderStatus Status,
    string? Notes,
    DateTime? SentAt,
    DateTime? ReceivedAt,
    List<DtfOrderItemDto> Items,
    DateTime CreatedAt);
```

**Commands:**

| Command | Campos | Ação |
|---|---|---|
| `CreateDtfOrderCommand` | `Notes?, Items: List<(DtfModelId, SheetQuantity)>` | Cria pedido em Draft |
| `UpdateDtfOrderCommand` | `Id, Notes?, Items: List<(DtfModelId, SheetQuantity)>` | Substitui itens e notas (Draft only) |
| `SendDtfOrderCommand` | `Id` | Draft → Sent |
| `ReceiveDtfOrderCommand` | `Id` | Sent → Received + cria DtfStockMovements |
| `CancelDtfOrderCommand` | `Id` | Soft delete (Draft ou Sent) |

**`ReceiveDtfOrderCommandHandler`** — para cada item:
1. Carrega `DtfStockItem` pelo `DtfModelId`
2. Chama `stockItem.AddMovement(DtfMovementType.Entrada, item.SheetQuantity, reason: $"Pedido DTF #{order.OrderNumber}", sheetCount: item.SheetQuantity)`
3. Chama `order.MarkReceived()`

**Query:**
- `GetDtfOrdersQuery` — retorna `List<DtfOrderDto>` filtrável por status, ordenado por OrderNumber desc

### Infrastructure

- `DtfOrderConfiguration.cs` — configura `DtfOrders` e `DtfOrderItems` com todas as propriedades; `Status` como string `HasConversion<string>()`
- Migration: `AddDtfOrders`

### API (`/api/dtf-orders`)

```
GET    /api/dtf-orders              → GetDtfOrdersQuery (query param: ?status=Draft|Sent|Received)
POST   /api/dtf-orders              → CreateDtfOrderCommand
PUT    /api/dtf-orders/{id}         → UpdateDtfOrderCommand (draft only)
POST   /api/dtf-orders/{id}/send    → SendDtfOrderCommand
POST   /api/dtf-orders/{id}/receive → ReceiveDtfOrderCommand
DELETE /api/dtf-orders/{id}         → CancelDtfOrderCommand
```

Erros de domínio retornam `400 { error: "mensagem" }`.

---

## Frontend

### Navegação

Nova tab `dtf-orders` adicionada ao sidebar sob **Produção**, entre "Pedidos de Costura" e "Lista de Separação".

### Tipos

```typescript
export type DtfOrderStatus = 'Draft' | 'Sent' | 'Received'

export interface DtfOrderItemDto {
  dtfModelId: string
  modelName: string
  sheetLabel: string
  sheetQuantity: number
  stampsPerSheet: number
  stampsTotal: number
}

export interface DtfOrderDto {
  id: string
  orderNumber: number
  status: DtfOrderStatus
  notes: string | null
  sentAt: string | null
  receivedAt: string | null
  items: DtfOrderItemDto[]
  createdAt: string
}
```

### Hooks

| Hook | Método | Endpoint |
|---|---|---|
| `useDtfOrders` | GET | `/dtf-orders` |
| `useCreateDtfOrder` | POST | `/dtf-orders` |
| `useUpdateDtfOrder` | PUT | `/dtf-orders/:id` |
| `useSendDtfOrder` | POST | `/dtf-orders/:id/send` |
| `useReceiveDtfOrder` | POST | `/dtf-orders/:id/receive` |
| `useCancelDtfOrder` | DELETE | `/dtf-orders/:id` |

Todos invalidam `['dtf-orders']` no `onSuccess`. `useReceiveDtfOrder` também invalida `['dtf-stock']`.

### Componentes (`features/dtf/orders/`)

**`DtfOrderPage`**
- Header: título "Pedidos DTF" + botão "+ Novo pedido"
- Lista de pedidos via `DtfOrderList`
- Dialogs: criação/edição (form), confirmação de envio, confirmação de recebimento, confirmação de cancelamento

**`DtfOrderList`**
- Tabela com colunas: Nº, Data, Modelos (resumo), Total de folhas, Status (badge), Ações
- Ações por status:
  - Draft: [Editar] [Marcar Enviado] [Cancelar]
  - Sent: [Marcar Recebido] [Cancelar]
  - Received: — (somente leitura)

**`DtfOrderForm`** (criação e edição)
- Campo Notas (textarea, opcional)
- Tabela de itens:
  - Dropdown: modelo DTF (lista de `DtfModel` ativos)
  - Input: quantidade de folhas
  - Preview calculado: `N folhas × M estampas/folha = X estampas`
  - Botão remover linha
- Botão "+ Adicionar modelo"
- Validação: mínimo 1 item, SheetQuantity > 0, sem modelo duplicado

**Confirmações simples (dialogs):**
- Enviar: "Marcar pedido #N como enviado?"
- Receber: "Marcar como recebido? O estoque DTF será atualizado automaticamente com X estampas totais."
- Cancelar: "Cancelar pedido #N? Esta ação não pode ser desfeita."

### Schema Zod

```typescript
const dtfOrderItemSchema = z.object({
  dtfModelId: z.string().uuid(),
  sheetQuantity: z.number().int().positive('Quantidade deve ser maior que zero'),
})

const dtfOrderSchema = z.object({
  notes: z.string().max(500).optional().nullable(),
  items: z.array(dtfOrderItemSchema).min(1, 'Adicione pelo menos um modelo'),
})
```

---

## O que este spec NÃO cobre (fora de escopo)

- Geração automática de Pedido DTF a partir da Lista de Separação
- Mensagem WhatsApp para o fornecedor DTF (pode ser adicionado depois)
- Relatório de histórico de compras DTF
- Preço/custo por pedido (custo por folha já existe no DtfModel)
