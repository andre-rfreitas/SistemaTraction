# Estorno Financeiro com Cancelamento em Cascata — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Quando um lançamento financeiro de entrega (Corte ou Costura) for estornado, o sistema cancela automaticamente o pedido de corte vinculado e reverte todos os efeitos colaterais (bobinas, estoque de camisetas, outros lançamentos do mesmo evento).

**Architecture:** Toda a lógica de cascata fica no `ReverseFinancialEntryCommandHandler` detectando o `ReferenceType` do lançamento. Uma única transação garante atomicidade. `CuttingOrderStatus` ganha o valor `Cancelled`; pedidos cancelados passam a aparecer na lista em vez de serem soft-deleted.

**Tech Stack:** .NET 9, C#, EF Core (InMemory para testes), xUnit + FluentAssertions, React + TypeScript, TanStack Query

---

## Mapa de arquivos

### Backend — modificados
- `backend/src/Domain/Cutting/CuttingOrderStatus.cs` — adicionar `Cancelled`
- `backend/src/Domain/Cutting/CuttingOrder.cs` — atualizar `Cancel()`, adicionar `CancelDelivered()`
- `backend/src/Domain/Fabric/FabricRoll.cs` — `RevertToAvailable()` aceitar `Consumed`
- `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs` — adaptar ao novo `Cancel()` sem soft-delete
- `backend/src/Application/Financial/Commands/ReverseFinancialEntry/ReverseFinancialEntryCommandHandler.cs` — lógica de cascata CuttingDelivery + SewingDelivery

### Backend — testes modificados
- `backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs` — novos casos de cascata

### Frontend — modificados
- `frontend/src/features/cutting/orders/types.ts` — adicionar `'Cancelled'` ao tipo status
- `frontend/src/features/cutting/orders/components/CuttingOrderList.tsx` — badge + ausência de ações para Cancelled
- `frontend/src/features/financial/components/EntriesTable.tsx` — aviso contextual no dialog de estorno
- `frontend/src/features/financial/hooks/useReverseFinancialEntry.ts` — invalidar queries adicionais

---

## Task 1: Domínio — CuttingOrderStatus, CuttingOrder e FabricRoll

**Files:**
- Modify: `backend/src/Domain/Cutting/CuttingOrderStatus.cs`
- Modify: `backend/src/Domain/Cutting/CuttingOrder.cs`
- Modify: `backend/src/Domain/Fabric/FabricRoll.cs`

- [ ] **Step 1: Adicionar `Cancelled` ao enum**

Abrir `backend/src/Domain/Cutting/CuttingOrderStatus.cs` e substituir o conteúdo:

```csharp
namespace SistemaTraction.Domain.Cutting;

public enum CuttingOrderStatus
{
    Draft,
    SentToCutter,
    Delivered,
    SewingDelivered,
    Cancelled
}
```

- [ ] **Step 2: Atualizar `Cancel()` e adicionar `CancelDelivered()` em `CuttingOrder`**

No arquivo `backend/src/Domain/Cutting/CuttingOrder.cs`, substituir o método `Cancel()` e adicionar `CancelDelivered()` logo abaixo dele:

```csharp
public void Cancel()
{
    if (Status == CuttingOrderStatus.Delivered || Status == CuttingOrderStatus.SewingDelivered)
        throw new DomainException("Pedidos já entregues não podem ser cancelados manualmente. Use o estorno financeiro.");
    Status = CuttingOrderStatus.Cancelled;
    TouchUpdatedAt();
}

public void CancelDelivered()
{
    Status = CuttingOrderStatus.Cancelled;
    TouchUpdatedAt();
}
```

- [ ] **Step 3: Atualizar `RevertToAvailable()` em `FabricRoll`**

No arquivo `backend/src/Domain/Fabric/FabricRoll.cs`, substituir o método `RevertToAvailable()`:

```csharp
public void RevertToAvailable()
{
    if (Status != FabricRollStatus.InCutting && Status != FabricRollStatus.Consumed)
        throw new DomainException("Bobina não pode ser revertida para disponível neste estado.");
    Status = FabricRollStatus.Available;
    TouchUpdatedAt();
}
```

- [ ] **Step 4: Compilar para verificar**

```bash
cd backend && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add backend/src/Domain/Cutting/CuttingOrderStatus.cs backend/src/Domain/Cutting/CuttingOrder.cs backend/src/Domain/Fabric/FabricRoll.cs
git commit -m "feat: adicionar status Cancelled e metodo CancelDelivered ao dominio"
```

---

## Task 2: Testes de domínio das novas regras

**Files:**
- Test: `backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs` (novos casos para domínio puro serão adicionados inline neste arquivo em Tasks futuras; aqui testamos o domínio sem DB)

Os testes de domínio puro não precisam de DbContext — instanciam as entidades diretamente.

- [ ] **Step 1: Criar arquivo de testes de domínio**

Criar `backend/tests/Application.Tests/Domain/CuttingOrderDomainTests.cs`:

```csharp
using FluentAssertions;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests.Domain;

public class CuttingOrderDomainTests
{
    private static CuttingOrder MakeOrder(CuttingOrderStatus targetStatus)
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        var order = CuttingOrder.Create(1, [(roll.Id, new Dictionary<string, int> { ["P"] = 5 })]);

        if (targetStatus == CuttingOrderStatus.Draft) return order;

        order.MarkSent();
        if (targetStatus == CuttingOrderStatus.SentToCutter) return order;

        order.MarkDelivered();
        if (targetStatus == CuttingOrderStatus.Delivered) return order;

        order.MarkSewingDelivered();
        return order;
    }

    [Fact]
    public void Cancel_Draft_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.Draft);
        order.Cancel();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
        order.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Cancel_SentToCutter_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.SentToCutter);
        order.Cancel();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Delivered_ThrowsDomainException()
    {
        var order = MakeOrder(CuttingOrderStatus.Delivered);
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*entregues*");
    }

    [Fact]
    public void Cancel_SewingDelivered_ThrowsDomainException()
    {
        var order = MakeOrder(CuttingOrderStatus.SewingDelivered);
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CancelDelivered_AnyStatus_SetsStatusCancelled()
    {
        var order = MakeOrder(CuttingOrderStatus.SewingDelivered);
        order.CancelDelivered();
        order.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public void FabricRoll_RevertToAvailable_FromConsumed_Succeeds()
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        roll.StartCutting();
        roll.MarkConsumed();
        roll.RevertToAvailable();
        roll.Status.Should().Be(FabricRollStatus.Available);
    }

    [Fact]
    public void FabricRoll_RevertToAvailable_FromAvailable_ThrowsDomainException()
    {
        var roll = FabricRoll.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, 200m);
        var act = () => roll.RevertToAvailable();
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 2: Executar os testes**

```bash
cd backend && dotnet test --filter "CuttingOrderDomainTests"
```
Expected: 7 tests, all PASS.

- [ ] **Step 3: Commit**

```bash
git add backend/tests/Application.Tests/Domain/CuttingOrderDomainTests.cs
git commit -m "test: testes de dominio para Cancel, CancelDelivered e FabricRoll.RevertToAvailable"
```

---

## Task 3: Atualizar `CancelCuttingOrderCommandHandler`

O `Cancel()` do domínio não faz mais soft-delete, apenas muda o status. O handler precisa refletir isso e garantir que bobinas em corte ainda sejam revertidas.

**Files:**
- Modify: `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs`

- [ ] **Step 1: Verificar handler atual**

Ler `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs`. O handler atual chama `order.Cancel()` que fazia `MarkAsDeleted()`. Agora `Cancel()` define `Status = Cancelled` — portanto nenhuma mudança na lógica de reversão de bobinas é necessária (o handler já reverte `FabricRoll` se `SentToCutter`).

O único ajuste: remover qualquer lógica que dependesse de `IsDeleted` pós-cancelamento. Substituir o conteúdo do handler:

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;

public class CancelCuttingOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelCuttingOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        if (order.Status == CuttingOrderStatus.SentToCutter)
        {
            foreach (var item in order.Items)
                item.FabricRoll?.RevertToAvailable();
        }

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

- [ ] **Step 2: Compilar e testar**

```bash
cd backend && dotnet build && dotnet test
```
Expected: Build succeeded, todos os testes passam.

- [ ] **Step 3: Commit**

```bash
git add backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs
git commit -m "refactor: cancelar pedido define status Cancelled em vez de soft-delete"
```

---

## Task 4: Cascata de estorno — `CuttingDelivery`

Estender o `ReverseFinancialEntryCommandHandler` para detectar `ReferenceType == "CuttingDelivery"` e:
1. Bloquear se há SewingDelivery vinculada
2. Reverter FabricRolls para Available
3. Cancelar o pedido com `CancelDelivered()`

**Files:**
- Modify: `backend/src/Application/Financial/Commands/ReverseFinancialEntry/ReverseFinancialEntryCommandHandler.cs`

- [ ] **Step 1: Escrever o teste de integração (falha primeiro)**

Adicionar ao arquivo `backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs` a classe auxiliar de seed e os novos testes:

```csharp
// No topo do arquivo, adicionar estes usings se ainda não existirem:
// using SistemaTraction.Domain.Cutting;
// using SistemaTraction.Domain.Fabric;

// Dentro da classe ReverseFinancialEntryCommandHandlerTests, adicionar:

private async Task<(FinancialEntry entry, CuttingOrder order, FabricRoll roll)> SeedCuttingDeliveryEntry()
{
    var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
    var fabricColor = fabricType.AddColor("Preto", "#000000");
    _context.FabricTypes.Add(fabricType);
    await _context.SaveChangesAsync();

    var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 10m, 200m);
    _context.FabricRolls.Add(roll);
    roll.StartCutting();
    await _context.SaveChangesAsync();

    var order = CuttingOrder.Create(1, [(roll.Id, new Dictionary<string, int> { ["P"] = 5 })]);
    _context.CuttingOrders.Add(order);
    await _context.SaveChangesAsync();

    order.MarkSent();
    await _context.SaveChangesAsync();

    var delivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 5 }, 5m);
    _context.CuttingDeliveries.Add(delivery);
    order.MarkDelivered();
    roll.MarkConsumed();
    await _context.SaveChangesAsync();

    var entry = FinancialEntry.CreateExpense("Corte", 5m, "Corte Pedido #1", delivery.Id, "CuttingDelivery");
    _context.FinancialEntries.Add(entry);
    await _context.SaveChangesAsync();

    return (entry, order, roll);
}

[Fact]
public async Task Handle_CuttingDelivery_CancelsCuttingOrderAndRevertsRoll()
{
    var (entry, order, roll) = await SeedCuttingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

    var updatedOrder = await _context.CuttingOrders.FindAsync(order.Id);
    var updatedRoll = await _context.FabricRolls.FindAsync(roll.Id);

    updatedOrder!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    updatedRoll!.Status.Should().Be(FabricRollStatus.Available);
}

[Fact]
public async Task Handle_CuttingDelivery_CreatesReversalEntry()
{
    var (entry, _, _) = await SeedCuttingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

    var reversal = _context.FinancialEntries.SingleOrDefault(e => e.IsReversal && e.ReferenceId == entry.Id);
    reversal.Should().NotBeNull();
    reversal!.Amount.Should().Be(-5m);
}

[Fact]
public async Task Handle_CuttingDelivery_WithSewingDelivery_ThrowsDomainException()
{
    var (entry, order, _) = await SeedCuttingDeliveryEntry();

    var sewingDelivery = SewingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 4 },
        new Dictionary<string, int>(), 22.4m, 0m);
    _context.SewingDeliveries.Add(sewingDelivery);
    order.MarkSewingDelivered();
    await _context.SaveChangesAsync();

    var act = () => _handler.Handle(new ReverseFinancialEntryCommand(entry.Id), CancellationToken.None);

    await act.Should().ThrowAsync<DomainException>().WithMessage("*costura*");
}
```

- [ ] **Step 2: Rodar os testes para verificar que falham**

```bash
cd backend && dotnet test --filter "Handle_CuttingDelivery"
```
Expected: 3 tests FAIL (lógica não implementada ainda).

- [ ] **Step 3: Implementar a lógica de cascata no handler**

Substituir o conteúdo completo de `backend/src/Application/Financial/Commands/ReverseFinancialEntry/ReverseFinancialEntryCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;

public class ReverseFinancialEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReverseFinancialEntryCommand, ReverseFinancialEntryResult>
{
    public async Task<ReverseFinancialEntryResult> Handle(ReverseFinancialEntryCommand request, CancellationToken cancellationToken)
    {
        var original = await context.FinancialEntries
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new DomainException("Lançamento financeiro não encontrado.");

        var alreadyReversed = await context.FinancialEntries
            .AnyAsync(e => e.IsReversal && e.ReferenceId == original.Id, cancellationToken);

        if (alreadyReversed)
            throw new DomainException("Este lançamento já foi estornado.");

        if (original.ReferenceType == "CuttingDelivery" && original.ReferenceId.HasValue)
            await ReverseCuttingDelivery(original.ReferenceId.Value, cancellationToken);
        else if (original.ReferenceType == "SewingDelivery" && original.ReferenceId.HasValue)
            await ReverseSewingDelivery(original.Id, original.ReferenceId.Value, cancellationToken);

        // Sempre cria o estorno do lançamento disparador — para SewingDelivery os outros
        // lançamentos vinculados são estornados dentro de ReverseSewingDelivery.
        var reversal = FinancialEntry.CreateReversal(original);
        context.FinancialEntries.Add(reversal);
        await context.SaveChangesAsync(cancellationToken);

        return new ReverseFinancialEntryResult(reversal.Id);
    }

    private async Task ReverseCuttingDelivery(Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await context.CuttingDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken)
            ?? throw new DomainException("Entrega de corte não encontrada.");

        var hasSewing = await context.SewingDeliveries
            .AnyAsync(s => s.CuttingOrderId == delivery.CuttingOrderId && !s.IsDeleted, cancellationToken);

        if (hasSewing)
            throw new DomainException("Estorno bloqueado: já existe entrega de costura para este pedido. Estorne a costura primeiro.");

        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == delivery.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        foreach (var item in order.Items)
            item.FabricRoll?.RevertToAvailable();

        order.CancelDelivered();
    }

    private async Task ReverseSewingDelivery(Guid triggeredEntryId, Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await context.SewingDeliveries
            .FirstOrDefaultAsync(s => s.Id == deliveryId, cancellationToken)
            ?? throw new DomainException("Entrega de costura não encontrada.");

        var order = await context.CuttingOrders
            .FirstOrDefaultAsync(o => o.Id == delivery.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        // Estornar os outros lançamentos do mesmo SewingDelivery (o disparador é estornado no Handle)
        var otherLinkedEntries = await context.FinancialEntries
            .Where(e => e.ReferenceId == deliveryId && e.ReferenceType == "SewingDelivery"
                        && e.Id != triggeredEntryId && !e.IsReversal)
            .ToListAsync(cancellationToken);

        foreach (var entry in otherLinkedEntries)
        {
            var alreadyReversed = await context.FinancialEntries
                .AnyAsync(e => e.IsReversal && e.ReferenceId == entry.Id, cancellationToken);
            if (!alreadyReversed)
                context.FinancialEntries.Add(FinancialEntry.CreateReversal(entry));
        }

        // Reverter estoque de camisetas
        var movements = await context.ShirtStockMovements
            .Where(m => m.ReferenceId == order.Id && m.Delta > 0)
            .ToListAsync(cancellationToken);

        foreach (var movement in movements)
        {
            var stockItem = await context.StockItems.FindAsync(movement.StockItemId);
            if (stockItem is null) continue;

            stockItem.UseFromStock(movement.Delta);
            context.ShirtStockMovements.Add(ShirtStockMovement.Create(
                movement.StockItemId,
                movement.FabricColorId,
                movement.FabricColorName,
                movement.Size,
                -movement.Delta,
                $"Estorno costura pedido #{order.OrderNumber}",
                "Estorno",
                order.Id));
        }

        order.CancelDelivered();
    }
}
```

- [ ] **Step 4: Rodar os testes**

```bash
cd backend && dotnet test --filter "Handle_CuttingDelivery"
```
Expected: 3 tests PASS.

- [ ] **Step 5: Rodar todos os testes para garantir que não quebrou nada**

```bash
cd backend && dotnet test
```
Expected: Todos os testes passam.

- [ ] **Step 6: Commit**

```bash
git add backend/src/Application/Financial/Commands/ReverseFinancialEntry/ReverseFinancialEntryCommandHandler.cs backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs
git commit -m "feat: estorno de CuttingDelivery cancela pedido e reverte bobinas"
```

---

## Task 5: Cascata de estorno — `SewingDelivery`

Adicionar testes para o caso `SewingDelivery` e garantir que o handler já implementado os passa.

**Files:**
- Test: `backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs`

- [ ] **Step 1: Adicionar método de seed para SewingDelivery**

Dentro da classe `ReverseFinancialEntryCommandHandlerTests`, adicionar:

```csharp
private async Task<(FinancialEntry sewingEntry, CuttingOrder order, Guid stockItemId)> SeedSewingDeliveryEntry()
{
    var fabricType = FabricType.Create("Malha", "Regular", 20m, 15m, null);
    var fabricColor = fabricType.AddColor("Preto", "#000000");
    _context.FabricTypes.Add(fabricType);
    await _context.SaveChangesAsync();

    var roll = FabricRoll.Create(fabricType.Id, fabricColor.Id, 10m, 200m);
    _context.FabricRolls.Add(roll);
    roll.StartCutting();
    await _context.SaveChangesAsync();

    var order = CuttingOrder.Create(2, [(roll.Id, new Dictionary<string, int> { ["P"] = 10 })]);
    _context.CuttingOrders.Add(order);
    await _context.SaveChangesAsync();

    order.MarkSent();
    var cuttingDelivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 10 }, 10m);
    _context.CuttingDeliveries.Add(cuttingDelivery);
    order.MarkDelivered();
    roll.MarkConsumed();
    await _context.SaveChangesAsync();

    // SewingDelivery
    var sewingDelivery = SewingDelivery.Create(order.Id,
        new Dictionary<string, int> { ["P"] = 9 },
        new Dictionary<string, int> { ["P"] = 1 },
        50.4m, 27m);
    _context.SewingDeliveries.Add(sewingDelivery);

    // StockItem + movimento
    var stockItem = StockItem.Create(fabricColor.Id, "Preto", "Malha", "Regular", "P", 9);
    _context.StockItems.Add(stockItem);
    await _context.SaveChangesAsync();

    _context.ShirtStockMovements.Add(ShirtStockMovement.Create(
        stockItem.Id, fabricColor.Id, "Preto", "P", 9,
        $"Costura pedido #{order.OrderNumber}", "Costureiro", order.Id));

    order.MarkSewingDelivered();
    await _context.SaveChangesAsync();

    // Lançamentos financeiros (Costura + Defeitos)
    var sewingEntry = FinancialEntry.CreateExpense("Costura", 50.4m, "Costura pedido #2 — 9 peças",
        sewingDelivery.Id, "SewingDelivery");
    var defectEntry = FinancialEntry.CreateExpense("Defeitos", 27m, "Defeitos pedido #2 — 1 peça",
        sewingDelivery.Id, "SewingDelivery");
    _context.FinancialEntries.AddRange(sewingEntry, defectEntry);
    await _context.SaveChangesAsync();

    return (sewingEntry, order, stockItem.Id);
}
```

- [ ] **Step 2: Adicionar os testes de SewingDelivery**

```csharp
[Fact]
public async Task Handle_SewingDelivery_CancelsCuttingOrder()
{
    var (sewingEntry, order, _) = await SeedSewingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);

    var updatedOrder = await _context.CuttingOrders.FindAsync(order.Id);
    updatedOrder!.Status.Should().Be(CuttingOrderStatus.Cancelled);
}

[Fact]
public async Task Handle_SewingDelivery_RevertsShirtStock()
{
    var (sewingEntry, _, stockItemId) = await SeedSewingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);

    var stockItem = await _context.StockItems.FindAsync(stockItemId);
    stockItem!.Quantity.Should().Be(0);
}

[Fact]
public async Task Handle_SewingDelivery_ReversesAllLinkedFinancialEntries()
{
    var (sewingEntry, _, _) = await SeedSewingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);

    var reversals = _context.FinancialEntries.Where(e => e.IsReversal).ToList();
    reversals.Should().HaveCount(2); // Costura + Defeitos
    reversals.Select(r => r.Amount).Should().Contain(-50.4m);
    reversals.Select(r => r.Amount).Should().Contain(-27m);
}

[Fact]
public async Task Handle_SewingDelivery_CreatesCompensatingStockMovement()
{
    var (sewingEntry, order, _) = await SeedSewingDeliveryEntry();

    await _handler.Handle(new ReverseFinancialEntryCommand(sewingEntry.Id), CancellationToken.None);

    var compensating = _context.ShirtStockMovements
        .Where(m => m.ReferenceId == order.Id && m.Delta < 0)
        .ToList();
    compensating.Should().HaveCount(1);
    compensating[0].Delta.Should().Be(-9);
    compensating[0].Origin.Should().Be("Estorno");
}
```

- [ ] **Step 3: Adicionar usings necessários no topo do arquivo de testes**

Verificar se os seguintes usings estão presentes no arquivo `ReverseFinancialEntryCommandHandlerTests.cs`:

```csharp
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;
```

- [ ] **Step 4: Rodar os testes de SewingDelivery**

```bash
cd backend && dotnet test --filter "Handle_SewingDelivery"
```
Expected: 4 tests PASS.

- [ ] **Step 5: Rodar todos os testes**

```bash
cd backend && dotnet test
```
Expected: Todos os testes passam.

- [ ] **Step 6: Commit**

```bash
git add backend/tests/Application.Tests/Financial/ReverseFinancialEntryCommandHandlerTests.cs
git commit -m "test: testes de cascata de estorno para SewingDelivery"
```

---

## Task 6: Frontend — tipo e badge de status Cancelled

**Files:**
- Modify: `frontend/src/features/cutting/orders/types.ts`
- Modify: `frontend/src/features/cutting/orders/components/CuttingOrderList.tsx`

- [ ] **Step 1: Adicionar `'Cancelled'` ao tipo status**

Em `frontend/src/features/cutting/orders/types.ts`, localizar:

```ts
status: 'Draft' | 'SentToCutter' | 'Delivered' | 'SewingDelivered'
```

Substituir por:

```ts
status: 'Draft' | 'SentToCutter' | 'Delivered' | 'SewingDelivered' | 'Cancelled'
```

- [ ] **Step 2: Adicionar label e variant para Cancelled em `CuttingOrderList.tsx`**

Em `frontend/src/features/cutting/orders/components/CuttingOrderList.tsx`, localizar:

```ts
const STATUS_LABEL: Record<string, string> = {
  Draft: 'Rascunho',
  SentToCutter: 'Enviado ao cortador',
  Delivered: 'Entregue pelo cortador',
  SewingDelivered: 'Em estoque',
}

const STATUS_VARIANT: Record<string, BadgeProps['variant']> = {
  Draft: 'neutral',
  SentToCutter: 'info',
  Delivered: 'warning',
  SewingDelivered: 'success',
}
```

Substituir por:

```ts
const STATUS_LABEL: Record<string, string> = {
  Draft: 'Rascunho',
  SentToCutter: 'Enviado ao cortador',
  Delivered: 'Entregue pelo cortador',
  SewingDelivered: 'Em estoque',
  Cancelled: 'Cancelado',
}

const STATUS_VARIANT: Record<string, BadgeProps['variant']> = {
  Draft: 'neutral',
  SentToCutter: 'info',
  Delivered: 'warning',
  SewingDelivered: 'success',
  Cancelled: 'danger',
}
```

- [ ] **Step 3: Impedir ações em pedidos cancelados**

Em `CuttingOrderList.tsx`, localizar:

```tsx
const canEdit = o.status === 'Draft'
const canCancel = o.status === 'Draft' || o.status === 'SentToCutter'
```

Substituir por:

```tsx
const isCancelled = o.status === 'Cancelled'
const canEdit = !isCancelled && o.status === 'Draft'
const canCancel = !isCancelled && (o.status === 'Draft' || o.status === 'SentToCutter')
```

- [ ] **Step 4: Verificar typecheck do frontend**

```bash
cd frontend && pnpm typecheck
```
Expected: 0 erros.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/features/cutting/orders/types.ts frontend/src/features/cutting/orders/components/CuttingOrderList.tsx
git commit -m "feat: exibir pedidos cancelados com badge na tela de pedidos de corte"
```

---

## Task 7: Frontend — aviso contextual no dialog de estorno e invalidação de queries

**Files:**
- Modify: `frontend/src/features/financial/components/EntriesTable.tsx`
- Modify: `frontend/src/features/financial/hooks/useReverseFinancialEntry.ts`

- [ ] **Step 1: Adicionar aviso no dialog de confirmação de estorno**

Em `frontend/src/features/financial/components/EntriesTable.tsx`, localizar o bloco dentro do `<Dialog>`:

```tsx
<div className="text-sm text-muted-foreground space-y-2">
  <p>
    Lançamentos financeiros não podem ser excluídos. O estorno cria um novo
    lançamento de valor oposto, mantendo a rastreabilidade.
  </p>
  {toReverse && (
    <div className="rounded-md bg-muted border p-3">
      <p className="font-medium text-foreground">{toReverse.description}</p>
      <p className="text-muted-foreground">
        {toReverse.category} — {formatBRL(toReverse.amount)}
      </p>
    </div>
  )}
</div>
```

Substituir por:

```tsx
<div className="text-sm text-muted-foreground space-y-2">
  <p>
    Lançamentos financeiros não podem ser excluídos. O estorno cria um novo
    lançamento de valor oposto, mantendo a rastreabilidade.
  </p>
  {toReverse && (
    <div className="rounded-md bg-muted border p-3">
      <p className="font-medium text-foreground">{toReverse.description}</p>
      <p className="text-muted-foreground">
        {toReverse.category} — {formatBRL(toReverse.amount)}
      </p>
    </div>
  )}
  {toReverse?.referenceType === 'CuttingDelivery' && (
    <p className="text-warning font-medium">
      Este estorno também cancelará o pedido de corte vinculado e reverterá as bobinas de tecido para disponível.
    </p>
  )}
  {toReverse?.referenceType === 'SewingDelivery' && (
    <p className="text-warning font-medium">
      Este estorno também cancelará o pedido de corte, reverterá o estoque de camisetas e estornará todos os lançamentos vinculados (Costura e Defeitos).
    </p>
  )}
</div>
```

- [ ] **Step 2: Atualizar `useReverseFinancialEntry` para invalidar queries adicionais**

Substituir o conteúdo de `frontend/src/features/financial/hooks/useReverseFinancialEntry.ts`:

```ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useReverseFinancialEntry() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => api.post(`/financial/entries/${id}/reverse`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financial-summary'] })
      queryClient.invalidateQueries({ queryKey: ['financial-entries'] })
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
      queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
    },
  })
}
```

- [ ] **Step 3: Verificar typecheck**

```bash
cd frontend && pnpm typecheck
```
Expected: 0 erros.

- [ ] **Step 4: Rodar lint**

```bash
cd frontend && pnpm lint
```
Expected: 0 warnings ou erros.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/features/financial/components/EntriesTable.tsx frontend/src/features/financial/hooks/useReverseFinancialEntry.ts
git commit -m "feat: aviso contextual no estorno financeiro e invalidacao de queries"
```

---

## Task 8: Verificação end-to-end

- [ ] **Step 1: Rodar todos os testes do backend**

```bash
cd backend && dotnet test --verbosity normal
```
Expected: Todos os testes passam, 0 falhas.

- [ ] **Step 2: Subir o backend**

```bash
cd backend && dotnet run --project src/API
```
Expected: API rodando em http://localhost:5000 sem erros.

- [ ] **Step 3: Subir o frontend**

```bash
cd frontend && pnpm dev
```
Expected: App rodando em http://localhost:5173.

- [ ] **Step 4: Testar cenário CuttingDelivery**

1. Criar um pedido de corte → enviar ao cortador → registrar entrega do corte
2. Ir para Financeiro → localizar o lançamento "Corte Pedido #N"
3. Clicar em "Estornar" → verificar que aparece o aviso sobre cancelamento do pedido
4. Confirmar o estorno
5. Verificar: lançamento aparece como estornado no financeiro
6. Verificar: pedido de corte aparece como "Cancelado" na tela de pedidos
7. Verificar: bobinas de tecido voltaram para "Disponível"

- [ ] **Step 5: Testar cenário SewingDelivery**

1. Criar pedido de corte → enviar → registrar entrega do corte → registrar entrega da costura
2. Ir para Financeiro → localizar o lançamento "Costura pedido #N"
3. Clicar em "Estornar" → verificar aviso sobre reversão de estoque e lançamentos
4. Confirmar o estorno
5. Verificar: lançamento "Costura" e lançamento "Defeitos" ambos aparecem como estornados
6. Verificar: pedido de corte aparece como "Cancelado"
7. Verificar: estoque de camisetas revertido (quantidade diminuída)

- [ ] **Step 6: Testar bloqueio de CuttingDelivery com SewingDelivery existente**

1. Criar pedido → entregar corte → entregar costura
2. Tentar estornar o lançamento de "Corte" (não o de Costura)
3. Verificar: erro 400 com mensagem sobre precisar estornar a costura primeiro

- [ ] **Step 7: Commit final se necessário**

Se houver ajustes visuais ou correções encontrados durante o teste manual:
```bash
git add -p
git commit -m "fix: ajustes pos-teste manual do estorno em cascata"
```
