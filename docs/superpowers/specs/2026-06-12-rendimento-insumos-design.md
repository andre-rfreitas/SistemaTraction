# Rendimento de Insumos — Design Spec

**Data:** 2026-06-12  
**Status:** Aprovado

## Contexto

Alguns insumos são consumidos por pedido (envelope, etiqueta térmica), outros por unidade de produto costurado (etiqueta de cetim, etiqueta de tamanho), e outros têm rendimento mais complexo (linha: 1 unidade rende 20 camisetas). O campo **Rendimento** permite registrar essa relação para que o sistema possa calcular saídas automaticamente no futuro.

## Modelo de Dados

Três colunas novas na tabela `SupplyTypes`:

| Campo | Tipo SQL | Tipo C# | Default |
|---|---|---|---|
| `YieldBasis` | `nvarchar(20)` (enum como string) | `YieldBasis` (enum) | `None` |
| `YieldQuantity` | `decimal(18,4) NULL` | `decimal?` | `null` |
| `YieldProductName` | `nvarchar(100) NULL` | `string?` | `null` |

### Enum `YieldBasis`

```csharp
public enum YieldBasis { None, PerOrder, PerProduct }
```

### Exemplos

| Insumo | YieldBasis | YieldQuantity | YieldProductName |
|---|---|---|---|
| Envelope | PerOrder | 1 | — |
| Etiqueta cetim | PerProduct | 1 | Camiseta |
| Linha | PerProduct | 20 | Camiseta |
| Agulha | None | — | — |

### Regras de domínio

- `YieldQuantity > 0` quando `YieldBasis ≠ None`
- `YieldProductName` obrigatório (não vazio) quando `YieldBasis = PerProduct`
- `YieldProductName` deve ser `null` quando `YieldBasis ≠ PerProduct`

## Backend

### Domain (`SistemaTraction.Domain`)

**Novo arquivo:** `Domain/Supplies/YieldBasis.cs`
```csharp
public enum YieldBasis { None, PerOrder, PerProduct }
```

**Modificado:** `Domain/Supplies/SupplyType.cs`
- Adicionar propriedades `YieldBasis`, `YieldQuantity`, `YieldProductName` (private set)
- Adicionar método `SetYield(YieldBasis basis, decimal quantity, string? productName)` com validações
- Adicionar método `ClearYield()` que reseta os três campos para `None / null / null`
- Atualizar `Create()` e `Update()` para aceitar os campos opcionais

### Application (`SistemaTraction.Application`)

**Modificado:** `Application/Supplies/DTOs/SupplyTypeDto.cs`
- Adicionar `YieldBasis YieldBasis`, `decimal? YieldQuantity`, `string? YieldProductName`

**Modificado:** `Application/Supplies/Commands/CreateSupplyType/CreateSupplyTypeCommand.cs`
- Adicionar `YieldBasis? YieldBasis`, `decimal? YieldQuantity`, `string? YieldProductName` (todos opcionais)

**Modificado:** `Application/Supplies/Commands/CreateSupplyType/CreateSupplyTypeCommandHandler.cs`
- Após criar o `SupplyType`, chamar `SetYield()` se `YieldBasis != null && != None`

**Modificado:** `Application/Supplies/Commands/UpdateSupplyType/UpdateSupplyTypeCommand.cs`
- Adicionar os mesmos 3 campos opcionais

**Modificado:** `Application/Supplies/Commands/UpdateSupplyType/UpdateSupplyTypeCommandHandler.cs`
- Chamar `SetYield()` ou `ClearYield()` conforme os valores recebidos

**Novo query:** `Application/Sewing/Queries/GetSewerProductTypeNames/`
- `GetSewerProductTypeNamesQuery` — `IRequest<List<string>>`
- Handler: busca todos os `SewerProductType` de costureiras ativas, retorna nomes distintos ordenados alfabeticamente

### API (`SistemaTraction.API`)

**Modificado:** `API/Controllers/SupplyTypesController.cs`
- `CreateSupplyTypeRequest` e `UpdateSupplyTypeRequest` ganham os 3 campos opcionais

**Modificado:** `API/Controllers/SewersController.cs`
- Novo endpoint: `GET /api/sewers/product-type-names` → `List<string>`

### Infrastructure

- Migration EF Core: adicionar `YieldBasis` (string, default `"None"`), `YieldQuantity` (decimal nullable), `YieldProductName` (string nullable, maxlength 100)
- Configuração no `ApplicationDbContext` / `SupplyTypeConfiguration`: converter enum `YieldBasis` como string

## Frontend

### Tipos (`types.ts`)

```typescript
export type YieldBasis = 'None' | 'PerOrder' | 'PerProduct'

export interface SupplyTypeDto {
  id: string
  name: string
  unit: string
  pricePerUnit: number | null
  yieldBasis: YieldBasis
  yieldQuantity: number | null
  yieldProductName: string | null
}
```

### Schema de validação (`supplyTypeSchema.ts`)

Adicionar campos opcionais com validação condicional:
- `yieldBasis`: enum opcional, default `'None'`
- `yieldQuantity`: number > 0, obrigatório quando `yieldBasis !== 'None'`
- `yieldProductName`: string obrigatória quando `yieldBasis === 'PerProduct'`

### Formulário (`SupplyTypeForm.tsx`)

Nova seção "Rendimento" abaixo dos campos existentes:

```
┌─ Rendimento (opcional) ──────────────────────────────────┐
│  1 unidade rende:  [1.00]  [Pedido ▾]                    │
│                            [Produto ▾] → [Camiseta ▾]    │
└──────────────────────────────────────────────────────────┘
```

- Quando `yieldBasis === 'None'`: exibe apenas botão/link "Configurar rendimento"
- Quando configurado: mostra os campos e um link "Remover"
- Dropdown "Produto" busca via hook `useSewerProductTypeNames`
- Se não há costureiras/produtos cadastrados, exibe mensagem informativa

### Lista (`SupplyTypeList.tsx`)

Nova coluna **Rendimento** na tabela:

| Configuração | Exibição |
|---|---|
| PerOrder, qty=1 | `1 un. rende 1 Pedido` |
| PerOrder, qty=5 | `1 un. rende 5 Pedidos` |
| PerProduct, qty=1, "Camiseta" | `1 un. rende 1 Camiseta` |
| PerProduct, qty=20, "Camiseta" | `1 un. rende 20 Camisetas` |
| None | `—` |

### Hook novo (`useSewerProductTypeNames.ts`)

```typescript
// GET /api/sewers/product-type-names → string[]
export function useSewerProductTypeNames(): UseQueryResult<string[]>
```

## Migration

```
dotnet ef migrations add AddSupplyTypeYield
dotnet ef database update
```

## O que este spec NÃO cobre (fora de escopo)

- Cálculo automático de saídas de insumos ao registrar pedidos de clientes (fase futura)
- Relatório de consumo projetado de insumos (fase futura)
- Validação de que `YieldProductName` existe nas costureiras cadastradas (tolerância a drift intencional)
