# Módulo de Estoque de Insumos (Embalagens)

**Data:** 2026-06-03  
**Status:** Aprovado

## Visão geral

Novo módulo `Supplies` para controle de estoque de insumos operacionais: envelopes de segurança, ziplock, etiquetas térmicas, etiquetas de tamanho, adesivos e panfletos. O módulo é independente, segue o padrão arquitetural já consolidado no projeto (CQRS, Clean Architecture, TanStack Query), e se integra com os módulos de Separação e Financeiro.

## Domínio e Banco de Dados

### Entidades

**`SupplyType`** — cadastro dinâmico de tipos de insumo:
- `Id` (Guid PK)
- `Name` (string) — ex: "Envelope de Segurança", "Etiqueta Térmica"
- `Unit` (string) — ex: "un", "pct"
- `IsActive` (bool)
- `CreatedAt`, `UpdatedAt`, `IsDeleted` (padrão do projeto)

**`SupplyStockItem`** — posição de estoque atual por tipo (uma linha por `SupplyType`):
- `Id` (Guid PK)
- `SupplyTypeId` (FK → `SupplyType`)
- `Quantity` (int)
- `CreatedAt`, `UpdatedAt`

**`SupplyStockMovement`** — histórico append-only de movimentações:
- `Id` (Guid PK)
- `SupplyStockItemId` (FK → `SupplyStockItem`)
- `Type` (enum: `Entry` | `Exit` | `Adjustment`)
- `Quantity` (int, pode ser negativo para saídas/ajustes)
- `Reason` (string?, motivo opcional)
- `CreatedAt`

**`SupplyOrderConfig`** — configuração de quantidade por pedido (usada no desconto automático):
- `Id` (Guid PK)
- `SupplyTypeId` (FK → `SupplyType`, unique)
- `QuantityPerOrder` (int, default 1)
- `CreatedAt`, `UpdatedAt`

### Regras de domínio
- Estoque nunca fica negativo — validado na entidade `SupplyStockItem`, não apenas na API
- `SupplyStockItem` é criado automaticamente quando um `SupplyType` é criado
- Movimentações são append-only — nunca atualizar, só inserir nova movimentação
- `SupplyType` com soft delete não aparece na listagem, mas seus dados históricos são preservados

## Backend (Application + API)

### Commands

| Command | Descrição |
|---|---|
| `CreateSupplyTypeCommand` | Cria tipo + cria `SupplyStockItem` com `Quantity = 0` |
| `UpdateSupplyTypeCommand` | Atualiza nome e/ou unidade do tipo |
| `DeleteSupplyTypeCommand` | Soft delete no tipo |
| `RegisterSupplyMovementCommand` | Registra entrada/saída/ajuste; impede negativar; entradas retornam `requiresFinancialConfirmation: true` |
| `UpsertSupplyOrderConfigCommand` | Salva ou atualiza a quantidade por pedido de um tipo |
| `DeductSuppliesForSeparationCommand` | Executa os descontos de estoque após confirmação do usuário na separação |

### Queries

| Query | Descrição |
|---|---|
| `GetSupplyTypesQuery` | Lista todos os tipos ativos |
| `GetSupplyStockItemsQuery` | Lista todos os tipos com saldo atual |
| `GetSupplyStockMovementsQuery` | Histórico de movimentações de um item |
| `GetSupplyOrderConfigsQuery` | Lê configuração atual de quantidade por pedido |
| `GetSupplyDeductionPreviewQuery` | Calcula preview de descontos para N pedidos (não executa) |

### Integração com Separação

O handler de `ConfirmSeparationListCommand` existente **não é alterado**. O fluxo é:

1. Frontend chama `GET /supplies/deduction-preview?orderCount=N` antes de confirmar
2. Backend retorna lista `[{ supplyTypeId, supplyTypeName, unit, quantity }]` calculada a partir de `SupplyOrderConfig`
3. Usuário revisa/edita e confirma → `POST /supplies/deduct-for-separation` com a lista final
4. Backend executa `DeductSuppliesForSeparationCommand` criando as movimentações

### Integração com Financeiro

Ao registrar uma **entrada** de estoque, o `RegisterSupplyMovementCommand` retorna no DTO de resposta:
```json
{
  "requiresFinancialConfirmation": true,
  "suggestedAmount": null,
  "suggestedDescription": "Compra: Envelope de Segurança (50 un)"
}
```
O frontend decide se cria o lançamento financeiro via `POST /financial/entries` (fluxo já existente). Nenhuma lógica financeira é executada automaticamente no backend para entradas — a categoria usada é o próprio nome do tipo (`supplyType.Name`).

### Endpoints da API

```
GET    /supply-types
POST   /supply-types
PUT    /supply-types/{id}
DELETE /supply-types/{id}

GET    /supply-stock
GET    /supply-stock/{id}/movements
POST   /supply-stock/{id}/movements

GET    /supply-order-configs
POST   /supply-order-configs          (upsert)

GET    /supplies/deduction-preview?orderCount={n}
POST   /supplies/deduct-for-separation
```

## Frontend

### Navegação (nav.ts)

Dois novos `TabId`:
- `supply-stock` → grupo **Estoque**, label "Embalagens", ícone `Package`
- `supply-types` → grupo **Cadastros**, label "Tipos de Insumo", ícone `Tag`

### Telas

**`SupplyStockPage`** (`features/stock/supplies/`)
- Tabela/cards com todos os tipos ativos e saldo atual
- Clica num item → Dialog com:
  1. Formulário: tipo de movimento (Entrada/Saída/Ajuste), quantidade, motivo opcional
  2. Histórico das últimas movimentações do item
  3. Após registrar **entrada**: segundo passo no mesmo Dialog — "Deseja registrar despesa no financeiro?" com campo de valor (obrigatório) e descrição pré-preenchida (editável). Usuário confirma ou descarta.

**`SupplyTypePage`** (`features/settings/supplies/`)
- CRUD idêntico ao padrão `DtfModelPage`
- Lista de tipos com ações de editar e excluir
- Formulário de criação/edição com campos: Nome, Unidade

**`SupplyOrderConfigSection`** (nova seção em `SettingsPage`)
- Lista todos os tipos ativos com campo numérico "Quantidade por pedido"
- Salva individualmente ao alterar (upsert)

### Modal de confirmação na Separação

Ao clicar em "Confirmar Lista" no `SeparationListPage`:
1. Chama `GET /supplies/deduction-preview?orderCount=N`
2. Abre Modal com tabela editável: `[nome do insumo | unidade | quantidade]`
3. Usuário pode ajustar quantidades antes de confirmar
4. Confirma → `POST /supplies/deduct-for-separation` → invalida `['supply-stock']`
5. Modal fecha e a confirmação da separação prossegue normalmente

### Hooks novos

```
features/settings/supplies/hooks/
  useSupplyTypes.ts
  useCreateSupplyType.ts
  useUpdateSupplyType.ts
  useDeleteSupplyType.ts

features/stock/supplies/hooks/
  useSupplyStock.ts
  useSupplyStockMovements.ts
  useRegisterSupplyMovement.ts

features/separation/hooks/
  useSupplyDeductionPreview.ts
  useDeductSuppliesForSeparation.ts

features/settings/config/hooks/
  useSupplyOrderConfigs.ts
  useUpsertSupplyOrderConfig.ts
```

### Schemas Zod novos

```
features/settings/supplies/schemas/supplyTypeSchema.ts
features/stock/supplies/schemas/supplyMovementSchema.ts
```

## Impacto em módulos existentes

| Módulo | Alteração |
|---|---|
| `SeparationListPage` | Adicionar chamada ao preview + modal de confirmação de descontos antes de confirmar lista |
| `SettingsPage` | Adicionar `SupplyOrderConfigSection` |
| `nav.ts` | Adicionar `supply-stock` e `supply-types` |
| `App.tsx` | Adicionar imports e entradas no `pages` record |
| `FinancialCategories.cs` | Nenhuma alteração — nome do tipo é usado diretamente como categoria |

Nenhuma entidade de domínio existente é modificada.

## Fora de escopo

- Alertas de estoque mínimo (pode ser adicionado futuramente)
- Relatórios específicos de consumo de embalagens por período
- Vínculo entre desconto de embalagens e pedidos de cliente individuais
