# Estoque DTF por Estampa — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrar o controle de estoque DTF de folhas para estampas como unidade canônica — entrada digitada em folhas e convertida (folhas × estampas/folha), saída e ajuste em estampas.

**Architecture:** A unidade canônica de `DtfStockItem.CurrentQuantity` e `DtfStockMovement.Delta` passa a ser estampa. A conversão folha→estampa acontece no handler de aplicação (que conhece o `DtfModel`); o domínio só recebe e valida estampas. O movimento de Entrada guarda o nº de folhas em `SheetCount` para o histórico. Frontend exibe estampas (com equivalência em folhas) e converte a entrada na UI.

**Tech Stack:** .NET 8 (Clean Architecture, MediatR, EF Core, FluentValidation, xUnit + FluentAssertions), React 18 + TypeScript (Vite, TanStack Query, React Hook Form + Zod).

**Pré-requisito:** trabalho na branch `feat/estoque-dtf-por-estampa` (já criada). Spec: `docs/superpowers/specs/2026-06-03-estoque-dtf-por-estampa-design.md`.

---

## Backend

### Task 1: Adicionar `SheetCount` ao `DtfStockMovement`

**Files:**
- Modify: `backend/src/Domain/Dtf/DtfStockMovement.cs`

- [ ] **Step 1: Adicionar a propriedade e o parâmetro na factory**

Em `backend/src/Domain/Dtf/DtfStockMovement.cs`, adicionar a propriedade `SheetCount` depois de `Reason` e incluí-la na factory `Create`. Arquivo completo resultante:

```csharp
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfStockMovement : BaseEntity
{
    public Guid DtfStockItemId { get; private set; }
    public DtfStockItem DtfStockItem { get; private set; } = null!;
    public DtfMovementType Type { get; private set; }

    /// <summary>Delta aplicado ao estoque em estampas: positivo = entrada, negativo = saída.</summary>
    public int Delta { get; private set; }

    public string? Reason { get; private set; }

    /// <summary>Número de folhas recebidas na Entrada. Null em Saída/Ajuste.</summary>
    public int? SheetCount { get; private set; }

    private DtfStockMovement() { }

    internal static DtfStockMovement Create(
        Guid stockItemId, DtfMovementType type, int delta, string? reason, int? sheetCount = null) =>
        new()
        {
            DtfStockItemId = stockItemId,
            Type = type,
            Delta = delta,
            Reason = reason?.Trim(),
            SheetCount = sheetCount
        };
}
```

- [ ] **Step 2: Compilar o domínio**

Run: `cd backend && dotnet build src/Domain`
Expected: build succeeds.

- [ ] **Step 3: Commit**

```bash
git add backend/src/Domain/Dtf/DtfStockMovement.cs
git commit -m "feat: adicionar SheetCount em DtfStockMovement"
```

---

### Task 2: `DtfStockItem.AddMovement` recebe estampas + sheetCount

**Files:**
- Modify: `backend/src/Domain/Dtf/DtfStockItem.cs`
- Modify: `backend/src/Domain/Dtf/DtfMovementType.cs`

- [ ] **Step 1: Atualizar comentários do enum**

Em `backend/src/Domain/Dtf/DtfMovementType.cs`, atualizar os comentários para refletir estampas:

```csharp
namespace SistemaTraction.Domain.Dtf;

public enum DtfMovementType
{
    Entrada = 1,  // estampas recebidas (entrada de folhas convertida)
    Saida = 2,    // estampas consumidas em produção
    Ajuste = 3    // correção manual de inventário (em estampas)
}
```

- [ ] **Step 2: Atualizar `AddMovement` para aceitar `sheetCount`**

Em `backend/src/Domain/Dtf/DtfStockItem.cs`, substituir o método `AddMovement`. O `quantity` recebido já está em estampas; o `sheetCount` é repassado ao movimento. Método resultante:

```csharp
    /// <summary>
    /// Registra uma movimentação de estoque em estampas (append-only).
    /// Para Entrada/Saida: quantity (estampas) deve ser positivo.
    /// Para Ajuste: quantity é o delta assinado em estampas (positivo ou negativo).
    /// sheetCount registra o nº de folhas na Entrada (null em Saida/Ajuste).
    /// </summary>
    public DtfStockMovement AddMovement(
        DtfMovementType type, int quantity, string? reason = null, int? sheetCount = null)
    {
        if (type != DtfMovementType.Ajuste && quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        if (type == DtfMovementType.Ajuste && quantity == 0)
            throw new DomainException("Delta de ajuste não pode ser zero.");

        var delta = type switch
        {
            DtfMovementType.Entrada => quantity,
            DtfMovementType.Saida   => -quantity,
            DtfMovementType.Ajuste  => quantity,
            _ => throw new DomainException("Tipo de movimentação inválido.")
        };

        if (CurrentQuantity + delta < 0)
            throw new DomainException(
                $"Estoque insuficiente. Atual: {CurrentQuantity}, solicitado: {quantity}.");

        CurrentQuantity += delta;
        TouchUpdatedAt();

        var movement = DtfStockMovement.Create(Id, type, delta, reason, sheetCount);
        _movements.Add(movement);
        return movement;
    }
```

- [ ] **Step 3: Compilar o domínio**

Run: `cd backend && dotnet build src/Domain`
Expected: build succeeds.

- [ ] **Step 4: Commit**

```bash
git add backend/src/Domain/Dtf/DtfStockItem.cs backend/src/Domain/Dtf/DtfMovementType.cs
git commit -m "feat: AddMovement em estampas com sheetCount opcional"
```

---

### Task 3: Handler converte folhas→estampas na Entrada

**Files:**
- Modify: `backend/src/Application/Dtf/Commands/RegisterDtfMovement/RegisterDtfMovementCommandHandler.cs`
- Test: `backend/tests/Application.Tests/Dtf/DtfStockTests.cs`

> Semântica do comando: para Entrada, `Quantity` = nº de folhas; para Saída/Ajuste, `Quantity` = estampas. O comando e o validator permanecem inalterados.

- [ ] **Step 1: Atualizar os testes existentes para a nova semântica**

O modelo de teste tem `StampsPerSheet = 9`. Substituir o corpo das asserções afetadas em `backend/tests/Application.Tests/Dtf/DtfStockTests.cs` (uma Entrada de `N` folhas → `N*9` estampas). Trocar os métodos abaixo:

```csharp
    [Fact]
    public async Task Entrada_PrimeiraMovimentacao_CriaStockItemEAtualiza()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, "Compra inicial"),
            CancellationToken.None);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(90); // 10 folhas * 9 estampas
    }

    [Fact]
    public async Task Entrada_SegundaMovimentacao_ReutilizaStockItemExistente()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, null),
            CancellationToken.None);

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 3, null),
            CancellationToken.None);

        var count = _context.DtfStockItems.Count(i => i.DtfModelId == _modelId);
        count.Should().Be(1, "deve existir apenas um StockItem por modelo");

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(72); // (5 + 3) folhas * 9
    }

    [Fact]
    public async Task Saida_ComEstoqueSuficiente_ReducStock()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 20, null),
            CancellationToken.None); // 180 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 7, "Produção"),
            CancellationToken.None); // -7 estampas

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(173);
    }

    [Fact]
    public async Task Saida_EstoqueInsuficiente_ThrowsDomainException()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, null),
            CancellationToken.None); // 45 estampas

        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 100, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*insuficiente*");
    }
```

E ajustar `AjustePositivo_IncrementaEstoque`, `AjusteNegativo_DecrementaEstoque`,
`AjusteNegativo_EstoqueInsuficiente_ThrowsDomainException`, `GetAll_RetornaItensComNomeDoModelo`,
`GetByModel_RetornaDetalheComMovimentos` e `Movimentos_SaoAppendOnly_NuncaAlterados` para a nova
escala. Versões corretas:

```csharp
    [Fact]
    public async Task AjustePositivo_IncrementaEstoque()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // 90 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, 5, "Recontagem"),
            CancellationToken.None); // +5 estampas

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(95);
    }

    [Fact]
    public async Task AjusteNegativo_DecrementaEstoque()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // 90 estampas

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, -3, "Perda"),
            CancellationToken.None);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(87);
    }

    [Fact]
    public async Task AjusteNegativo_EstoqueInsuficiente_ThrowsDomainException()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 1, null),
            CancellationToken.None); // 9 estampas

        var act = () => _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Ajuste, -50, "Erro"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public async Task GetAll_RetornaItensComNomeDoModelo()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None);

        var items = await _getAllHandler.Handle(new GetDtfStockItemsQuery(), CancellationToken.None);

        items.Should().HaveCount(1);
        items[0].ModelName.Should().Be("Angel");
        items[0].SheetLabel.Should().Be("Folha 1");
        items[0].CurrentQuantity.Should().Be(90);
        items[0].StampsPerSheet.Should().Be(9);
    }

    [Fact]
    public async Task GetByModel_RetornaDetalheComMovimentos()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 20, "Compra"),
            CancellationToken.None); // +180 estampas, SheetCount 20

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 5, "Produção"),
            CancellationToken.None); // -5 estampas

        var detail = await _getByModelHandler.Handle(
            new GetDtfStockItemByModelQuery(_modelId), CancellationToken.None);

        detail.Should().NotBeNull();
        detail!.Item.CurrentQuantity.Should().Be(175);
        detail.Movements.Should().HaveCount(2);
        detail.Movements[0].Delta.Should().Be(-5);  // mais recente primeiro
        detail.Movements[0].SheetCount.Should().BeNull();
        detail.Movements[1].Delta.Should().Be(180);
        detail.Movements[1].SheetCount.Should().Be(20);
    }

    [Fact]
    public async Task Movimentos_SaoAppendOnly_NuncaAlterados()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 10, null),
            CancellationToken.None); // +90

        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Saida, 3, null),
            CancellationToken.None); // -3

        var movements = _context.DtfStockMovements.ToList();
        movements.Should().HaveCount(2, "cada operação gera exatamente um registro novo");
        movements.Select(m => m.Delta).Should().BeEquivalentTo([90, -3]);
    }
```

Adicionar também um teste novo cobrindo explicitamente a conversão e o `SheetCount`:

```csharp
    [Fact]
    public async Task Entrada_ConverteFolhasParaEstampasERegistraSheetCount()
    {
        await _registerHandler.Handle(
            new RegisterDtfMovementCommand(_modelId, DtfMovementType.Entrada, 5, "Compra"),
            CancellationToken.None);

        var movement = _context.DtfStockMovements.Single();
        movement.Delta.Should().Be(45);       // 5 folhas * 9 estampas
        movement.SheetCount.Should().Be(5);

        var item = _context.DtfStockItems.First(i => i.DtfModelId == _modelId);
        item.CurrentQuantity.Should().Be(45);
    }
```

- [ ] **Step 2: Rodar os testes para vê-los falhar**

Run: `cd backend && dotnet test --filter "FullyQualifiedName~DtfStockTests"`
Expected: FAIL — handler ainda não converte folhas→estampas (Entrada de 5 grava 5, não 45) e `StampsPerSheet`/`SheetCount` não existem nos DTOs (compilação pode falhar; nesse caso conclua a Task 4 e re-execute). Se a compilação falhar por causa de `StampsPerSheet`/`SheetCount` nos DTOs, implemente a Task 4 antes de re-rodar.

- [ ] **Step 3: Implementar a conversão no handler**

Substituir `backend/src/Application/Dtf/Commands/RegisterDtfMovement/RegisterDtfMovementCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;

public class RegisterDtfMovementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterDtfMovementCommand, Guid>
{
    public async Task<Guid> Handle(RegisterDtfMovementCommand request, CancellationToken cancellationToken)
    {
        var model = await context.DtfModels
            .FirstOrDefaultAsync(m => m.Id == request.DtfModelId && !m.IsDeleted, cancellationToken)
            ?? throw new DomainException("Modelo DTF não encontrado.");

        var stockItem = await context.DtfStockItems
            .FirstOrDefaultAsync(i => i.DtfModelId == request.DtfModelId && !i.IsDeleted, cancellationToken);

        if (stockItem is null)
        {
            stockItem = DtfStockItem.Create(request.DtfModelId);
            context.DtfStockItems.Add(stockItem);
        }

        DtfStockMovement movement;
        if (request.Type == DtfMovementType.Entrada)
        {
            var stamps = request.Quantity * model.StampsPerSheet;
            movement = stockItem.AddMovement(
                DtfMovementType.Entrada, stamps, request.Reason, sheetCount: request.Quantity);
        }
        else
        {
            movement = stockItem.AddMovement(request.Type, request.Quantity, request.Reason);
        }

        context.DtfStockMovements.Add(movement);

        await context.SaveChangesAsync(cancellationToken);

        return movement.Id;
    }
}
```

- [ ] **Step 4: Rodar os testes (precisa da Task 4 concluída)**

Run: `cd backend && dotnet test --filter "FullyQualifiedName~DtfStockTests"`
Expected: PASS (após a Task 4 ter adicionado `StampsPerSheet`/`SheetCount` aos DTOs).

- [ ] **Step 5: Commit**

```bash
git add backend/src/Application/Dtf/Commands/RegisterDtfMovement/RegisterDtfMovementCommandHandler.cs backend/tests/Application.Tests/Dtf/DtfStockTests.cs
git commit -m "feat: handler converte folhas em estampas na entrada DTF"
```

---

### Task 4: DTOs e queries expõem `StampsPerSheet` e `SheetCount`

**Files:**
- Modify: `backend/src/Application/Dtf/DTOs/DtfStockItemDto.cs`
- Modify: `backend/src/Application/Dtf/DTOs/DtfStockMovementDto.cs`
- Modify: `backend/src/Application/Dtf/Queries/GetDtfStockItems/GetDtfStockItemsQueryHandler.cs`
- Modify: `backend/src/Application/Dtf/Queries/GetDtfStockItemByModel/GetDtfStockItemByModelQueryHandler.cs`

- [ ] **Step 1: Adicionar `StampsPerSheet` ao `DtfStockItemDto`**

```csharp
namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfStockItemDto(
    Guid Id,
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int CurrentQuantity,
    int StampsPerSheet
);
```

- [ ] **Step 2: Adicionar `SheetCount` ao `DtfStockMovementDto`**

```csharp
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfStockMovementDto(
    Guid Id,
    DtfMovementType Type,
    int Delta,
    string? Reason,
    DateTime CreatedAt,
    int? SheetCount
);
```

- [ ] **Step 3: Projetar `StampsPerSheet` em `GetDtfStockItemsQueryHandler`**

Substituir o `.Select(...)`:

```csharp
            .Select(i => new DtfStockItemDto(
                i.Id,
                i.DtfModelId,
                i.DtfModel.Name,
                i.DtfModel.SheetLabel,
                i.CurrentQuantity,
                i.DtfModel.StampsPerSheet))
```

- [ ] **Step 4: Projetar os novos campos em `GetDtfStockItemByModelQueryHandler`**

Atualizar a projeção do item e a dos movimentos:

```csharp
            .Select(i => new DtfStockItemDto(
                i.Id,
                i.DtfModelId,
                i.DtfModel.Name,
                i.DtfModel.SheetLabel,
                i.CurrentQuantity,
                i.DtfModel.StampsPerSheet))
```

e

```csharp
            .Select(m => new DtfStockMovementDto(m.Id, m.Type, m.Delta, m.Reason, m.CreatedAt, m.SheetCount))
```

- [ ] **Step 5: Compilar e rodar todos os testes DTF**

Run: `cd backend && dotnet build && dotnet test --filter "FullyQualifiedName~Dtf"`
Expected: build succeeds; testes DTF PASS (inclui os da Task 3).

- [ ] **Step 6: Commit**

```bash
git add backend/src/Application/Dtf/DTOs/ backend/src/Application/Dtf/Queries/
git commit -m "feat: DTOs DTF expoem StampsPerSheet e SheetCount"
```

---

### Task 5: EF config, config de alerta dedicada e migration

**Files:**
- Modify: `backend/src/Infrastructure/Persistence/Configurations/DtfStockMovementConfiguration.cs`
- Modify: `backend/src/Infrastructure/Persistence/Configurations/AppConfigConfiguration.cs`

- [ ] **Step 1: Mapear `SheetCount` na configuração EF**

Em `DtfStockMovementConfiguration.cs`, adicionar a propriedade (nullable, não obrigatória) após `Reason`:

```csharp
        builder.Property(m => m.Reason).HasMaxLength(500);
        builder.Property(m => m.SheetCount);
```

- [ ] **Step 2: Adicionar o seed da config `dtf_stock_alert_threshold`**

Em `AppConfigConfiguration.cs`, dentro de `builder.HasData(...)`, adicionar uma nova entrada com Guid inédito (após a entrada `wp_template_dtf` `...000d`):

```csharp
            ,Seed(new Guid("cccccccc-0000-0000-0000-00000000000e"),
                "dtf_stock_alert_threshold", "100",
                "Quantidade mínima de estampas DTF antes de disparar alerta de reposição")
```

> Inserir a vírgula corretamente: a entrada anterior (`wp_template_dtf`) deixa de ser a última, então a nova linha acima já começa com a vírgula necessária. Garanta que o `)` final de `HasData(` continue fechando após esta entrada.

- [ ] **Step 3: Gerar a migration**

Run: `cd backend && dotnet ef migrations add AddDtfSheetCountAndDtfThreshold --project src/Infrastructure --startup-project src/API`
Expected: migration criada em `src/Infrastructure/Persistence/Migrations` adicionando a coluna `SheetCount` em `DtfStockMovements` e inserindo o novo `AppConfig`.

- [ ] **Step 4: Compilar para validar a migration**

Run: `cd backend && dotnet build`
Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add backend/src/Infrastructure/Persistence/
git commit -m "feat: migration SheetCount + config dtf_stock_alert_threshold"
```

---

## Frontend

### Task 6: Types TS dos DTOs DTF

**Files:**
- Modify: `frontend/src/features/stock/dtf/types.ts`

- [ ] **Step 1: Adicionar `stampsPerSheet` e `sheetCount`**

Atualizar as interfaces no topo de `frontend/src/features/stock/dtf/types.ts`:

```ts
export interface DtfStockItemDto {
  id: string
  dtfModelId: string
  modelName: string
  sheetLabel: string
  currentQuantity: number
  stampsPerSheet: number
}

export interface DtfStockMovementDto {
  id: string
  type: DtfMovementType
  delta: number
  reason: string | null
  createdAt: string
  sheetCount: number | null
}
```

(Manter `DtfStockItemDetailDto`, `MOVEMENT_TYPE_LABEL` e `MOVEMENT_TYPE_CLASS` inalterados.)

- [ ] **Step 2: Typecheck**

Run: `cd frontend && pnpm typecheck`
Expected: erros apenas nos componentes que ainda não usam os novos campos (corrigidos nas tasks seguintes). Se `pnpm typecheck` falhar só por uso futuro, prossiga; nenhum erro deve vir de `types.ts`.

- [ ] **Step 3: Commit**

```bash
git add frontend/src/features/stock/dtf/types.ts
git commit -m "feat: types DTF com stampsPerSheet e sheetCount"
```

---

### Task 7: Formulário de movimento — entrada em folhas com preview

**Files:**
- Modify: `frontend/src/features/stock/dtf/components/RegisterMovementForm.tsx`
- Modify: `frontend/src/features/stock/dtf/DtfStockPage.tsx`
- Modify: `frontend/src/features/stock/dtf/components/DtfStockList.tsx`

> O form precisa do `stampsPerSheet` do modelo selecionado para o preview "= N estampas". Esse valor virá do item selecionado, que passa a carregar `stampsPerSheet` sempre.

- [ ] **Step 1: `DtfStockList` inclui `stampsPerSheet` no objeto selecionado**

Em `frontend/src/features/stock/dtf/components/DtfStockList.tsx`:

Atualizar a assinatura de `onSelect` na interface `Props`:

```tsx
interface Props {
  onSelect: (
    item:
      | DtfStockItemDto
      | { dtfModelId: string; modelName: string; sheetLabel: string; stampsPerSheet: number }
  ) => void
}
```

E no fallback do `onClick`, incluir `stampsPerSheet`:

```tsx
                onSelect(
                  stock ?? {
                    dtfModelId: model.id,
                    modelName: model.name,
                    sheetLabel: model.sheetLabel,
                    stampsPerSheet: model.stampsPerSheet,
                  }
                )
```

Também trocar o bloco de exibição da quantidade (label e equivalência em folhas):

```tsx
            <div className="flex items-center gap-3 shrink-0">
              <div className="text-right">
                <p className={`text-2xl font-bold tabular-nums ${isLow ? 'text-danger' : 'text-foreground'}`}>
                  {qty}
                </p>
                <p className="text-xs text-muted-foreground">estampas</p>
                <p className="text-xs text-muted-foreground">
                  ≈ {Math.floor(qty / model.stampsPerSheet)} folhas
                </p>
                {isLow && hasStock && (
                  <p className="text-xs text-warning font-medium">⚠ estoque baixo</p>
                )}
              </div>
```

E trocar a fonte do threshold para a config dedicada:

```tsx
  const threshold = Number(
    configs?.find((c) => c.key === 'dtf_stock_alert_threshold')?.value ?? 100
  )
```

- [ ] **Step 2: `DtfStockPage` repassa `stampsPerSheet` ao form**

Em `frontend/src/features/stock/dtf/DtfStockPage.tsx`, atualizar o tipo `SelectedItem` e passar `stampsPerSheet` para o `RegisterMovementForm`:

```tsx
type SelectedItem =
  | DtfStockItemDto
  | { dtfModelId: string; modelName: string; sheetLabel: string; stampsPerSheet: number }
```

No JSX do form, adicionar a prop:

```tsx
              <RegisterMovementForm
                stampsPerSheet={selected?.stampsPerSheet ?? 1}
                isLoading={register.isPending}
                onSubmit={(data) => {
                  if (!selected) return
                  register.mutate(
                    {
                      dtfModelId: selected.dtfModelId,
                      type: data.type as DtfMovementType,
                      quantity: data.quantity,
                      reason: data.reason || null,
                    },
                    { onSuccess: handleClose }
                  )
                }}
              />
```

E atualizar o texto do `PageHeader`:

```tsx
      <PageHeader
        title="Estoque de DTF"
        description="Posição atual em estampas por modelo. Registre entradas (em folhas), saídas e ajustes."
      />
```

- [ ] **Step 3: `RegisterMovementForm` — label dinâmico + preview**

Substituir `frontend/src/features/stock/dtf/components/RegisterMovementForm.tsx`:

```tsx
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  movementSchema,
  type MovementFormData,
  type MovementFormInput,
} from '../schemas/movementSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  stampsPerSheet: number
  onSubmit: (data: MovementFormData) => void
  isLoading?: boolean
}

export function RegisterMovementForm({ stampsPerSheet, onSubmit, isLoading }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<MovementFormInput, unknown, MovementFormData>({
    resolver: zodResolver(movementSchema),
    defaultValues: { type: 1, quantity: 1, reason: '' },
  })

  const type = Number(useWatch({ control, name: 'type' }))
  const quantity = Number(useWatch({ control, name: 'quantity' })) || 0
  const isEntrada = type === 1

  return (
    <form
      onSubmit={handleSubmit((data) => {
        onSubmit(data)
        reset()
      })}
      className="space-y-4"
    >
      <div className="space-y-1">
        <Label htmlFor="type">Tipo de movimento</Label>
        <select
          id="type"
          {...register('type')}
          className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
        >
          <option value={1}>Entrada — recebimento de folhas</option>
          <option value={2}>Saída — estampas usadas em produção</option>
          <option value={3}>Ajuste — correção de inventário (estampas)</option>
        </select>
        {errors.type && (
          <p className="text-sm text-danger">{errors.type.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="quantity">
          {isEntrada
            ? 'Folhas recebidas'
            : type === 3
              ? 'Estampas (use negativo para reduzir)'
              : 'Estampas'}
        </Label>
        <Input
          id="quantity"
          type="number"
          min={type === 3 ? undefined : 1}
          step={1}
          {...register('quantity')}
        />
        {isEntrada && quantity > 0 && (
          <p className="text-xs text-muted-foreground">
            = {quantity * stampsPerSheet} estampas ({stampsPerSheet} por folha)
          </p>
        )}
        {errors.quantity && (
          <p className="text-sm text-danger">{errors.quantity.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="reason">Motivo (opcional)</Label>
        <Input id="reason" {...register('reason')} placeholder="ex: Compra #42, Produção lote 7" />
        {errors.reason && (
          <p className="text-sm text-danger">{errors.reason.message}</p>
        )}
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Registrando...' : 'Registrar movimento'}
      </Button>
    </form>
  )
}
```

- [ ] **Step 4: Typecheck e lint**

Run: `cd frontend && pnpm typecheck && pnpm lint`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/features/stock/dtf/components/RegisterMovementForm.tsx frontend/src/features/stock/dtf/DtfStockPage.tsx frontend/src/features/stock/dtf/components/DtfStockList.tsx
git commit -m "feat: entrada DTF em folhas com preview e lista em estampas"
```

---

### Task 8: Histórico mostra estampas (e folhas na entrada)

**Files:**
- Modify: `frontend/src/features/stock/dtf/components/DtfStockDetail.tsx`

- [ ] **Step 1: Exibir unidade e folhas na entrada**

Em `frontend/src/features/stock/dtf/components/DtfStockDetail.tsx`, dentro do `.map`, substituir o bloco que renderiza `{sign}{m.delta}` para incluir "estampas" e, na entrada, o nº de folhas:

```tsx
              <span className={`font-mono font-semibold ${cls}`}>
                {sign}{m.delta} estampas
              </span>
              {m.sheetCount != null && (
                <span className="text-muted-foreground text-xs">
                  ({m.sheetCount} folhas)
                </span>
              )}
```

(O restante do componente permanece igual.)

- [ ] **Step 2: Typecheck e lint**

Run: `cd frontend && pnpm typecheck && pnpm lint`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add frontend/src/features/stock/dtf/components/DtfStockDetail.tsx
git commit -m "feat: historico DTF em estampas com folhas na entrada"
```

---

### Task 9: Verificação final

- [ ] **Step 1: Backend completo**

Run: `cd backend && dotnet build && dotnet test`
Expected: build succeeds; todos os testes PASS.

- [ ] **Step 2: Frontend completo**

Run: `cd frontend && pnpm typecheck && pnpm lint && pnpm build`
Expected: typecheck, lint e build PASS.

- [ ] **Step 3: Revisão manual rápida (opcional, recomendada)**

Subir API (`dotnet run --project src/API`) e frontend (`pnpm dev`), abrir a tela de Estoque DTF, registrar uma Entrada de 5 folhas num modelo com 10 estampas/folha e confirmar: estoque = 50 estampas, histórico "+50 estampas (5 folhas)", lista mostra "50 estampas ≈ 5 folhas".

---

## Notas de implementação

- **Ordem das Tasks 3 e 4:** os testes da Task 3 dependem de campos adicionados na Task 4 (`StampsPerSheet`/`SheetCount` nos DTOs). Se executar estritamente em ordem, a compilação dos testes falhará no Step 2 da Task 3 — isso é esperado; complete a Task 4 e re-rode (Step 4 da Task 3). Alternativamente, execute a Task 4 antes da Task 3.
- **Vírgula no seed (Task 5):** atenção ao inserir a nova entrada em `HasData` sem quebrar a lista de argumentos.
- **Sem migração de dados:** não há conversão de quantidades existentes (confirmado: sem dados reais).
- **Threshold compartilhado intacto:** `stock_alert_threshold` (estoque de camisetas) não é alterado.
