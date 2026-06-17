# Pedidos DTF Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar o módulo Pedidos DTF — criação manual de pedidos de compra de folhas DTF junto ao fornecedor, com atualização automática do estoque ao receber.

**Architecture:** DtfOrder é um aggregate root em `Domain/Dtf/` seguindo o mesmo padrão de CuttingOrder. Ao marcar como recebido, o handler cria movimentações de estoque DTF via `DtfStockItem.AddMovement()`. Frontend em `features/dtf/orders/` com tab `dtf-orders` na seção Produção do sidebar.

**Tech Stack:** .NET 8 C# (MediatR CQRS, EF Core, FluentValidation), React 18 + TypeScript, TanStack Query, React Hook Form + Zod, shadcn/ui, Tailwind CSS.

---

## File Map

**Criar:**
- `backend/src/Domain/Dtf/DtfOrderStatus.cs`
- `backend/src/Domain/Dtf/DtfOrderItem.cs`
- `backend/src/Domain/Dtf/DtfOrder.cs`
- `backend/tests/Application.Tests/Dtf/DtfOrderTests.cs`
- `backend/src/Application/Dtf/DTOs/DtfOrderDto.cs`
- `backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommand.cs`
- `backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommandHandler.cs`
- `backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommand.cs`
- `backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommandHandler.cs`
- `backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommand.cs`
- `backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommandHandler.cs`
- `backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommand.cs`
- `backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommandHandler.cs`
- `backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommand.cs`
- `backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommandHandler.cs`
- `backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQuery.cs`
- `backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQueryHandler.cs`
- `backend/src/Infrastructure/Persistence/Configurations/DtfOrderConfiguration.cs`
- `frontend/src/features/dtf/orders/types.ts`
- `frontend/src/features/dtf/orders/hooks/useDtfOrders.ts`
- `frontend/src/features/dtf/orders/hooks/useCreateDtfOrder.ts`
- `frontend/src/features/dtf/orders/hooks/useUpdateDtfOrder.ts`
- `frontend/src/features/dtf/orders/hooks/useSendDtfOrder.ts`
- `frontend/src/features/dtf/orders/hooks/useReceiveDtfOrder.ts`
- `frontend/src/features/dtf/orders/hooks/useCancelDtfOrder.ts`
- `frontend/src/features/dtf/orders/schemas/dtfOrderSchema.ts`
- `frontend/src/features/dtf/orders/components/DtfOrderForm.tsx`
- `frontend/src/features/dtf/orders/components/DtfOrderList.tsx`
- `frontend/src/features/dtf/orders/DtfOrderPage.tsx`

**Modificar:**
- `backend/src/Application/Common/Interfaces/IApplicationDbContext.cs` — adicionar DbSet<DtfOrder> e DbSet<DtfOrderItem>
- `backend/tests/Application.Tests/TestApplicationDbContext.cs` — adicionar DbSets e config EF para DtfOrder/DtfOrderItem
- `backend/src/Infrastructure/Persistence/ApplicationDbContext.cs` — adicionar DbSets
- `frontend/src/components/layout/nav.ts` — adicionar tab `dtf-orders`
- `frontend/src/App.tsx` — adicionar TabId e import da DtfOrderPage

---

### Task 1: Domain — DtfOrderStatus, DtfOrderItem e DtfOrder

**Files:**
- Create: `backend/src/Domain/Dtf/DtfOrderStatus.cs`
- Create: `backend/src/Domain/Dtf/DtfOrderItem.cs`
- Create: `backend/src/Domain/Dtf/DtfOrder.cs`
- Test: `backend/tests/Application.Tests/Dtf/DtfOrderTests.cs`

- [ ] **Step 1: Criar DtfOrderStatus.cs**

```csharp
// backend/src/Domain/Dtf/DtfOrderStatus.cs
namespace SistemaTraction.Domain.Dtf;

public enum DtfOrderStatus { Draft, Sent, Received }
```

- [ ] **Step 2: Criar DtfOrderItem.cs**

```csharp
// backend/src/Domain/Dtf/DtfOrderItem.cs
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfOrderItem : BaseEntity
{
    public Guid DtfOrderId { get; private set; }
    public Guid DtfModelId { get; private set; }
    public int SheetQuantity { get; private set; }

    private DtfOrderItem() { }

    public static DtfOrderItem Create(Guid dtfOrderId, Guid dtfModelId, int sheetQuantity)
    {
        if (sheetQuantity <= 0)
            throw new DomainException("Quantidade de folhas deve ser maior que zero.");

        return new DtfOrderItem
        {
            DtfOrderId = dtfOrderId,
            DtfModelId = dtfModelId,
            SheetQuantity = sheetQuantity,
        };
    }

    public void UpdateSheetQuantity(int sheetQuantity)
    {
        if (sheetQuantity <= 0)
            throw new DomainException("Quantidade de folhas deve ser maior que zero.");
        SheetQuantity = sheetQuantity;
        TouchUpdatedAt();
    }
}
```

- [ ] **Step 3: Escrever testes que vão falhar**

```csharp
// backend/tests/Application.Tests/Dtf/DtfOrderTests.cs
using FluentAssertions;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;
using Xunit;

namespace SistemaTraction.Application.Tests.Dtf;

public class DtfOrderTests
{
    [Fact]
    public void Create_WithItems_CreatesInDraftStatus()
    {
        var items = new List<(Guid, int)> { (Guid.NewGuid(), 10) };
        var order = DtfOrder.Create(1, items, "Nota teste");
        order.Status.Should().Be(DtfOrderStatus.Draft);
        order.OrderNumber.Should().Be(1);
        order.Notes.Should().Be("Nota teste");
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyItems_ThrowsDomainException()
    {
        var act = () => DtfOrder.Create(1, new List<(Guid, int)>(), null);
        act.Should().Throw<DomainException>().WithMessage("*pelo menos um modelo*");
    }

    [Fact]
    public void Create_WithDuplicateModel_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var items = new List<(Guid, int)> { (modelId, 5), (modelId, 3) };
        var act = () => DtfOrder.Create(1, items, null);
        act.Should().Throw<DomainException>().WithMessage("*duplicado*");
    }

    [Fact]
    public void AddItem_ToDraftOrder_AddsItem()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        var newModelId = Guid.NewGuid();
        order.AddItem(newModelId, 3);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_DuplicateModel_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5) }, null);
        var act = () => order.AddItem(modelId, 3);
        act.Should().Throw<DomainException>().WithMessage("*duplicado*");
    }

    [Fact]
    public void RemoveItem_FromDraftOrder_RemovesItem()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5), (Guid.NewGuid(), 3) }, null);
        order.RemoveItem(modelId);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_FromSentOrder_ThrowsDomainException()
    {
        var modelId = Guid.NewGuid();
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (modelId, 5) }, null);
        order.MarkSent();
        var act = () => order.RemoveItem(modelId);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkSent_FromDraft_TransitionsToSent()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.Status.Should().Be(DtfOrderStatus.Sent);
        order.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkSent_FromSent_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        var act = () => order.MarkSent();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkReceived_FromSent_TransitionsToReceived()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.MarkReceived();
        order.Status.Should().Be(DtfOrderStatus.Received);
        order.ReceivedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkReceived_FromDraft_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        var act = () => order.MarkReceived();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_InDraft_SetsIsDeleted()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.Cancel();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Cancel_InSent_SetsIsDeleted()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.Cancel();
        order.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Cancel_InReceived_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        order.MarkReceived();
        var act = () => order.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*recebido*");
    }

    [Fact]
    public void UpdateNotes_InDraft_UpdatesNotes()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.UpdateNotes("nova nota");
        order.Notes.Should().Be("nova nota");
    }

    [Fact]
    public void UpdateNotes_InSent_ThrowsDomainException()
    {
        var order = DtfOrder.Create(1, new List<(Guid, int)> { (Guid.NewGuid(), 5) }, null);
        order.MarkSent();
        var act = () => order.UpdateNotes("nova nota");
        act.Should().Throw<DomainException>();
    }
}
```

- [ ] **Step 4: Executar testes para verificar que falham**

```
cd backend
dotnet test tests/Application.Tests --filter "FullyQualifiedName~DtfOrderTests" -v
```

Esperado: FAIL — `DtfOrder` não existe.

- [ ] **Step 5: Criar DtfOrder.cs**

```csharp
// backend/src/Domain/Dtf/DtfOrder.cs
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfOrder : BaseEntity
{
    public int OrderNumber { get; private set; }
    public DtfOrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }

    private readonly List<DtfOrderItem> _items = [];
    public IReadOnlyList<DtfOrderItem> Items => _items.AsReadOnly();

    private DtfOrder() { }

    public static DtfOrder Create(int orderNumber, List<(Guid DtfModelId, int SheetQuantity)> items, string? notes)
    {
        if (items.Count == 0)
            throw new DomainException("O pedido deve ter pelo menos um modelo.");

        var distinctModels = items.Select(i => i.DtfModelId).Distinct().Count();
        if (distinctModels != items.Count)
            throw new DomainException("Não é permitido adicionar o mesmo modelo duplicado no pedido.");

        var order = new DtfOrder
        {
            OrderNumber = orderNumber,
            Status = DtfOrderStatus.Draft,
            Notes = notes?.Trim(),
        };

        foreach (var (modelId, sheetQty) in items)
            order._items.Add(DtfOrderItem.Create(order.Id, modelId, sheetQty));

        return order;
    }

    public void AddItem(Guid dtfModelId, int sheetQuantity)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Itens só podem ser alterados em pedidos em rascunho.");

        if (_items.Any(i => i.DtfModelId == dtfModelId && !i.IsDeleted))
            throw new DomainException("Não é permitido adicionar o mesmo modelo duplicado no pedido.");

        _items.Add(DtfOrderItem.Create(Id, dtfModelId, sheetQuantity));
        TouchUpdatedAt();
    }

    public void RemoveItem(Guid dtfModelId)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Itens só podem ser alterados em pedidos em rascunho.");

        var item = _items.FirstOrDefault(i => i.DtfModelId == dtfModelId && !i.IsDeleted)
            ?? throw new DomainException("Modelo não encontrado no pedido.");

        item.MarkAsDeleted();
        TouchUpdatedAt();
    }

    public void UpdateNotes(string? notes)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Notas só podem ser alteradas em pedidos em rascunho.");
        Notes = notes?.Trim();
        TouchUpdatedAt();
    }

    public void MarkSent()
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Apenas pedidos em rascunho podem ser enviados.");

        Status = DtfOrderStatus.Sent;
        SentAt = DateTime.UtcNow;
        TouchUpdatedAt();
    }

    public void MarkReceived()
    {
        if (Status != DtfOrderStatus.Sent)
            throw new DomainException("Apenas pedidos enviados podem ser marcados como recebidos.");

        Status = DtfOrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        TouchUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == DtfOrderStatus.Received)
            throw new DomainException("Pedidos já recebidos não podem ser cancelados.");
        MarkAsDeleted();
    }
}
```

- [ ] **Step 6: Executar testes**

```
cd backend
dotnet test tests/Application.Tests --filter "FullyQualifiedName~DtfOrderTests" -v
```

Esperado: todos os testes passando.

- [ ] **Step 7: Build completo**

```
cd backend
dotnet build
```

Esperado: Build succeeded.

- [ ] **Step 8: Commit**

```
git add backend/src/Domain/Dtf/DtfOrderStatus.cs backend/src/Domain/Dtf/DtfOrderItem.cs backend/src/Domain/Dtf/DtfOrder.cs backend/tests/Application.Tests/Dtf/DtfOrderTests.cs
git commit -m "feat: domain DtfOrder aggregate — DtfOrderStatus, DtfOrderItem, DtfOrder"
```

---

### Task 2: Application — DTOs, Commands e GetDtfOrdersQuery

**Files:**
- Create: `backend/src/Application/Dtf/DTOs/DtfOrderDto.cs`
- Create: `backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommand.cs`
- Create: `backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommandHandler.cs`
- Create: `backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommand.cs`
- Create: `backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommandHandler.cs`
- Create: `backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommand.cs`
- Create: `backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommandHandler.cs`
- Create: `backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommand.cs`
- Create: `backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommandHandler.cs`
- Create: `backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommand.cs`
- Create: `backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommandHandler.cs`
- Create: `backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQuery.cs`
- Create: `backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQueryHandler.cs`
- Modify: `backend/src/Application/Common/Interfaces/IApplicationDbContext.cs`

- [ ] **Step 1: Criar DTOs**

```csharp
// backend/src/Application/Dtf/DTOs/DtfOrderDto.cs
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfOrderItemDto(
    Guid Id,
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int SheetQuantity,
    int StampsPerSheet,
    int StampsTotal);

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

- [ ] **Step 2: Atualizar IApplicationDbContext**

Adicionar antes de `Task<int> SaveChangesAsync(...)`:

```csharp
DbSet<DtfOrder> DtfOrders { get; }
DbSet<DtfOrderItem> DtfOrderItems { get; }
```

Adicionar os usings necessários (já devem estar presentes via `using SistemaTraction.Domain.Dtf;`).

- [ ] **Step 3: Criar CreateDtfOrderCommand**

```csharp
// backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommand.cs
using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;

public record CreateDtfOrderItemInput(Guid DtfModelId, int SheetQuantity);

public record CreateDtfOrderCommand(
    string? Notes,
    List<CreateDtfOrderItemInput> Items) : IRequest<Guid>;
```

- [ ] **Step 4: Criar CreateDtfOrderCommandHandler**

```csharp
// backend/src/Application/Dtf/Commands/CreateDtfOrder/CreateDtfOrderCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;

public class CreateDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDtfOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = await context.DtfOrders
            .Where(o => !o.IsDeleted)
            .MaxAsync(o => (int?)o.OrderNumber, cancellationToken) ?? 0;
        orderNumber++;

        var items = request.Items
            .Select(i => (i.DtfModelId, i.SheetQuantity))
            .ToList();

        var order = DtfOrder.Create(orderNumber, items, request.Notes);
        context.DtfOrders.Add(order);

        await context.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
```

- [ ] **Step 5: Criar UpdateDtfOrderCommand e Handler**

```csharp
// backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommand.cs
using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;

public record UpdateDtfOrderItemInput(Guid DtfModelId, int SheetQuantity);

public record UpdateDtfOrderCommand(
    Guid Id,
    string? Notes,
    List<UpdateDtfOrderItemInput> Items) : IRequest;
```

```csharp
// backend/src/Application/Dtf/Commands/UpdateDtfOrder/UpdateDtfOrderCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;

public class UpdateDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDtfOrderCommand>
{
    public async Task Handle(UpdateDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.UpdateNotes(request.Notes);

        var requestedModelIds = request.Items.Select(i => i.DtfModelId).ToHashSet();
        var existingModelIds = order.Items.Where(i => !i.IsDeleted).Select(i => i.DtfModelId).ToHashSet();

        foreach (var existingItem in order.Items.Where(i => !i.IsDeleted))
        {
            if (!requestedModelIds.Contains(existingItem.DtfModelId))
                order.RemoveItem(existingItem.DtfModelId);
        }

        foreach (var input in request.Items)
        {
            var existing = order.Items.FirstOrDefault(i => i.DtfModelId == input.DtfModelId && !i.IsDeleted);
            if (existing is not null)
                existing.UpdateSheetQuantity(input.SheetQuantity);
            else
                order.AddItem(input.DtfModelId, input.SheetQuantity);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 6: Criar SendDtfOrderCommand e Handler**

```csharp
// backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommand.cs
using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.SendDtfOrder;

public record SendDtfOrderCommand(Guid Id) : IRequest;
```

```csharp
// backend/src/Application/Dtf/Commands/SendDtfOrder/SendDtfOrderCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.SendDtfOrder;

public class SendDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SendDtfOrderCommand>
{
    public async Task Handle(SendDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.MarkSent();
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 7: Criar ReceiveDtfOrderCommand e Handler**

```csharp
// backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommand.cs
using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;

public record ReceiveDtfOrderCommand(Guid Id) : IRequest;
```

```csharp
// backend/src/Application/Dtf/Commands/ReceiveDtfOrder/ReceiveDtfOrderCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;

public class ReceiveDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReceiveDtfOrderCommand>
{
    public async Task Handle(ReceiveDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        var activeItems = order.Items.Where(i => !i.IsDeleted).ToList();

        foreach (var item in activeItems)
        {
            var model = await context.DtfModels
                .FirstOrDefaultAsync(m => m.Id == item.DtfModelId && !m.IsDeleted, cancellationToken)
                ?? throw new DomainException($"Modelo DTF não encontrado para item do pedido.");

            var stockItem = await context.DtfStockItems
                .FirstOrDefaultAsync(s => s.DtfModelId == item.DtfModelId && !s.IsDeleted, cancellationToken);

            if (stockItem is null)
            {
                stockItem = DtfStockItem.Create(item.DtfModelId);
                context.DtfStockItems.Add(stockItem);
            }

            int stamps;
            try { stamps = checked(item.SheetQuantity * model.StampsPerSheet); }
            catch (OverflowException) { throw new DomainException("Quantidade de folhas muito alta para conversão em estampas."); }

            var movement = stockItem.AddMovement(
                DtfMovementType.Entrada,
                stamps,
                reason: $"Pedido DTF #{order.OrderNumber}",
                sheetCount: item.SheetQuantity);

            context.DtfStockMovements.Add(movement);
        }

        order.MarkReceived();
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 8: Criar CancelDtfOrderCommand e Handler**

```csharp
// backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommand.cs
using MediatR;

namespace SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;

public record CancelDtfOrderCommand(Guid Id) : IRequest;
```

```csharp
// backend/src/Application/Dtf/Commands/CancelDtfOrder/CancelDtfOrderCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;

public class CancelDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelDtfOrderCommand>
{
    public async Task Handle(CancelDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 9: Criar GetDtfOrdersQuery e Handler**

```csharp
// backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQuery.cs
using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfOrders;

public record GetDtfOrdersQuery(string? Status) : IRequest<List<DtfOrderDto>>;
```

```csharp
// backend/src/Application/Dtf/Queries/GetDtfOrders/GetDtfOrdersQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfOrders;

public class GetDtfOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfOrdersQuery, List<DtfOrderDto>>
{
    public async Task<List<DtfOrderDto>> Handle(GetDtfOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.DtfOrders
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<DtfOrderStatus>(request.Status, out var statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        var modelIds = orders.SelectMany(o => o.Items)
            .Where(i => !i.IsDeleted)
            .Select(i => i.DtfModelId)
            .Distinct()
            .ToList();

        var models = await context.DtfModels
            .Where(m => modelIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        return orders.Select(o => new DtfOrderDto(
            o.Id,
            o.OrderNumber,
            o.Status,
            o.Notes,
            o.SentAt,
            o.ReceivedAt,
            o.Items.Where(i => !i.IsDeleted).Select(i =>
            {
                var model = models.GetValueOrDefault(i.DtfModelId);
                return new DtfOrderItemDto(
                    i.Id,
                    i.DtfModelId,
                    model?.Name ?? "Modelo removido",
                    model?.SheetLabel ?? "-",
                    i.SheetQuantity,
                    model?.StampsPerSheet ?? 0,
                    i.SheetQuantity * (model?.StampsPerSheet ?? 0));
            }).ToList(),
            o.CreatedAt
        )).ToList();
    }
}
```

- [ ] **Step 10: Build**

```
cd backend
dotnet build
```

Esperado: Build succeeded.

- [ ] **Step 11: Commit**

```
git add backend/src/Application/Dtf/ backend/src/Application/Common/Interfaces/IApplicationDbContext.cs
git commit -m "feat: application layer Pedidos DTF — DTOs, 5 commands, GetDtfOrdersQuery"
```

---

### Task 3: Infrastructure — EF Config, ApplicationDbContext e Migration

**Files:**
- Create: `backend/src/Infrastructure/Persistence/Configurations/DtfOrderConfiguration.cs`
- Modify: `backend/src/Infrastructure/Persistence/ApplicationDbContext.cs`

- [ ] **Step 1: Criar DtfOrderConfiguration.cs**

```csharp
// backend/src/Infrastructure/Persistence/Configurations/DtfOrderConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfOrderConfiguration : IEntityTypeConfiguration<DtfOrder>
{
    public void Configure(EntityTypeBuilder<DtfOrder> builder)
    {
        builder.ToTable("DtfOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(i => i.DtfOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
               .HasField("_items")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class DtfOrderItemConfiguration : IEntityTypeConfiguration<DtfOrderItem>
{
    public void Configure(EntityTypeBuilder<DtfOrderItem> builder)
    {
        builder.ToTable("DtfOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SheetQuantity).IsRequired();

        builder.HasOne<DtfModel>()
               .WithMany()
               .HasForeignKey(i => i.DtfModelId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
```

- [ ] **Step 2: Adicionar DbSets em ApplicationDbContext.cs**

Localizar o arquivo em `backend/src/Infrastructure/Persistence/ApplicationDbContext.cs`. Adicionar as duas propriedades junto com as outras DTF:

```csharp
public DbSet<DtfOrder> DtfOrders => Set<DtfOrder>();
public DbSet<DtfOrderItem> DtfOrderItems => Set<DtfOrderItem>();
```

- [ ] **Step 3: Gerar migration**

```
cd backend
dotnet ef migrations add AddDtfOrders --project src/Infrastructure --startup-project src/API
```

Verificar que a migration foi criada em `src/Infrastructure/Persistence/Migrations/`.

- [ ] **Step 4: Aplicar migration**

```
cd backend
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

- [ ] **Step 5: Build e testes**

```
cd backend
dotnet build && dotnet test
```

Esperado: Build succeeded, todos os testes passando.

- [ ] **Step 6: Commit**

```
git add backend/src/Infrastructure/ backend/src/Infrastructure/Persistence/Migrations/
git commit -m "feat: infrastructure DtfOrders — EF config, migration AddDtfOrders"
```

---

### Task 4: API Controller e TestApplicationDbContext

**Files:**
- Create: `backend/src/API/Controllers/DtfOrdersController.cs`
- Modify: `backend/tests/Application.Tests/TestApplicationDbContext.cs`

- [ ] **Step 1: Atualizar TestApplicationDbContext**

Adicionar os dois DbSets após `DtfStockMovements`:

```csharp
public DbSet<DtfOrder> DtfOrders => Set<DtfOrder>();
public DbSet<DtfOrderItem> DtfOrderItems => Set<DtfOrderItem>();
```

Adicionar configuração EF no `OnModelCreating`, após o bloco de `DtfStockMovement`:

```csharp
modelBuilder.Entity<DtfOrder>(b =>
{
    b.HasKey(o => o.Id);
    b.Property(o => o.Status).HasConversion<string>();
    b.HasMany(o => o.Items)
     .WithOne()
     .HasForeignKey(i => i.DtfOrderId);
    b.Navigation(o => o.Items)
     .HasField("_items")
     .UsePropertyAccessMode(PropertyAccessMode.Field);
});

modelBuilder.Entity<DtfOrderItem>(b =>
{
    b.HasKey(i => i.Id);
    b.HasOne<DtfModel>()
     .WithMany()
     .HasForeignKey(i => i.DtfModelId)
     .IsRequired(false);
});
```

- [ ] **Step 2: Criar DtfOrdersController.cs**

```csharp
// backend/src/API/Controllers/DtfOrdersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;
using SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;
using SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;
using SistemaTraction.Application.Dtf.Commands.SendDtfOrder;
using SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;
using SistemaTraction.Application.Dtf.Queries.GetDtfOrders;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dtf-orders")]
public class DtfOrdersController(IMediator mediator) : ControllerBase
{
    // GET api/dtf-orders?status=Draft
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDtfOrdersQuery(status), ct);
        return Ok(result);
    }

    // POST api/dtf-orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDtfOrderRequest request, CancellationToken ct)
    {
        try
        {
            var items = request.Items
                .Select(i => new CreateDtfOrderItemInput(i.DtfModelId, i.SheetQuantity))
                .ToList();
            var id = await mediator.Send(new CreateDtfOrderCommand(request.Notes, items), ct);
            return Ok(new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/dtf-orders/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDtfOrderRequest request, CancellationToken ct)
    {
        try
        {
            var items = request.Items
                .Select(i => new UpdateDtfOrderItemInput(i.DtfModelId, i.SheetQuantity))
                .ToList();
            await mediator.Send(new UpdateDtfOrderCommand(id, request.Notes, items), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/dtf-orders/{id}/send
    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new SendDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/dtf-orders/{id}/receive
    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new ReceiveDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/dtf-orders/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new CancelDtfOrderCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateDtfOrderItemRequest(Guid DtfModelId, int SheetQuantity);
public record CreateDtfOrderRequest(string? Notes, List<CreateDtfOrderItemRequest> Items);

public record UpdateDtfOrderItemRequest(Guid DtfModelId, int SheetQuantity);
public record UpdateDtfOrderRequest(string? Notes, List<UpdateDtfOrderItemRequest> Items);
```

- [ ] **Step 3: Build e testes**

```
cd backend
dotnet build && dotnet test
```

Esperado: Build succeeded, todos os testes passando.

- [ ] **Step 4: Commit**

```
git add backend/src/API/Controllers/DtfOrdersController.cs backend/tests/Application.Tests/TestApplicationDbContext.cs
git commit -m "feat: API DtfOrdersController e TestApplicationDbContext atualizado"
```

---

### Task 5: Frontend — Types, Schema e Hooks

**Files:**
- Create: `frontend/src/features/dtf/orders/types.ts`
- Create: `frontend/src/features/dtf/orders/schemas/dtfOrderSchema.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useDtfOrders.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useCreateDtfOrder.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useUpdateDtfOrder.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useSendDtfOrder.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useReceiveDtfOrder.ts`
- Create: `frontend/src/features/dtf/orders/hooks/useCancelDtfOrder.ts`

- [ ] **Step 1: Criar types.ts**

```typescript
// frontend/src/features/dtf/orders/types.ts
export type DtfOrderStatus = 'Draft' | 'Sent' | 'Received'

export interface DtfOrderItemDto {
  id: string
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

- [ ] **Step 2: Criar dtfOrderSchema.ts**

```typescript
// frontend/src/features/dtf/orders/schemas/dtfOrderSchema.ts
import { z } from 'zod'

export const dtfOrderItemSchema = z.object({
  dtfModelId: z.string().uuid('Selecione um modelo'),
  sheetQuantity: z.number().int().positive('Quantidade deve ser maior que zero'),
})

export const dtfOrderSchema = z.object({
  notes: z.string().max(500).optional().nullable(),
  items: z.array(dtfOrderItemSchema).min(1, 'Adicione pelo menos um modelo'),
})

export type DtfOrderFormData = z.infer<typeof dtfOrderSchema>
export type DtfOrderItemFormData = z.infer<typeof dtfOrderItemSchema>
```

- [ ] **Step 3: Criar useDtfOrders.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useDtfOrders.ts
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderDto, DtfOrderStatus } from '../types'

export function useDtfOrders(status?: DtfOrderStatus) {
  return useQuery({
    queryKey: ['dtf-orders', status],
    queryFn: async () => {
      const params = status ? { status } : undefined
      const { data } = await api.get<DtfOrderDto[]>('/dtf-orders', { params })
      return data
    },
  })
}
```

- [ ] **Step 4: Criar useCreateDtfOrder.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useCreateDtfOrder.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderFormData } from '../schemas/dtfOrderSchema'

export function useCreateDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: DtfOrderFormData) => api.post('/dtf-orders', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
```

- [ ] **Step 5: Criar useUpdateDtfOrder.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useUpdateDtfOrder.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { DtfOrderFormData } from '../schemas/dtfOrderSchema'

export function useUpdateDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: DtfOrderFormData }) =>
      api.put(`/dtf-orders/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
```

- [ ] **Step 6: Criar useSendDtfOrder.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useSendDtfOrder.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useSendDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.post(`/dtf-orders/${id}/send`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
```

- [ ] **Step 7: Criar useReceiveDtfOrder.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useReceiveDtfOrder.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useReceiveDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.post(`/dtf-orders/${id}/receive`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dtf-orders'] })
      queryClient.invalidateQueries({ queryKey: ['dtf-stock'] })
    },
  })
}
```

- [ ] **Step 8: Criar useCancelDtfOrder.ts**

```typescript
// frontend/src/features/dtf/orders/hooks/useCancelDtfOrder.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useCancelDtfOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/dtf-orders/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dtf-orders'] }),
  })
}
```

- [ ] **Step 9: Typecheck**

```
cd frontend
pnpm typecheck
```

Esperado: sem erros.

- [ ] **Step 10: Commit**

```
git add frontend/src/features/dtf/orders/
git commit -m "feat: frontend Pedidos DTF — types, schema Zod, 6 hooks TanStack Query"
```

---

### Task 6: Frontend — DtfOrderForm

**Files:**
- Create: `frontend/src/features/dtf/orders/components/DtfOrderForm.tsx`

O formulário usa `useFieldArray` do React Hook Form para a lista de itens. Cada linha tem um dropdown de modelo DTF e input de quantidade. Requer `useDtfModels` do módulo existente em `features/settings/dtf/`.

- [ ] **Step 1: Verificar hook useDtfModels existente**

Verificar que `frontend/src/features/settings/dtf/hooks/useDtfModels.ts` existe e retorna `DtfModelDto[]` com campos `id`, `name`, `sheetLabel`, `stampsPerSheet`. Se não existir ou tiver nome diferente, buscar com:

```
grep -r "useDtfModels\|DtfModel" frontend/src/features --include="*.ts" -l
```

- [ ] **Step 2: Criar DtfOrderForm.tsx**

```typescript
// frontend/src/features/dtf/orders/components/DtfOrderForm.tsx
import { useForm, useFieldArray, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { dtfOrderSchema, type DtfOrderFormData } from '../schemas/dtfOrderSchema'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import type { DtfOrderDto } from '../types'

interface DtfModelOption {
  id: string
  name: string
  sheetLabel: string
  stampsPerSheet: number
}

interface Props {
  models: DtfModelOption[]
  defaultValues?: DtfOrderDto
  isLoading: boolean
  onSubmit: (data: DtfOrderFormData) => void
}

export function DtfOrderForm({ models, defaultValues, isLoading, onSubmit }: Props) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<DtfOrderFormData>({
    resolver: zodResolver(dtfOrderSchema),
    defaultValues: defaultValues
      ? {
          notes: defaultValues.notes ?? '',
          items: defaultValues.items.map(i => ({
            dtfModelId: i.dtfModelId,
            sheetQuantity: i.sheetQuantity,
          })),
        }
      : { notes: '', items: [{ dtfModelId: '', sheetQuantity: 1 }] },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'items' })
  const watchedItems = useWatch({ control, name: 'items' })

  function getModelById(id: string) {
    return models.find(m => m.id === id)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-1">
        <label className="text-sm font-medium text-foreground">Observações</label>
        <textarea
          {...register('notes')}
          rows={2}
          placeholder="Opcional"
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring resize-none"
        />
        {errors.notes && <p className="text-xs text-danger">{errors.notes.message}</p>}
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Modelos</label>
        {fields.map((field, index) => {
          const selectedModelId = watchedItems?.[index]?.dtfModelId
          const selectedModel = selectedModelId ? getModelById(selectedModelId) : undefined
          const qty = watchedItems?.[index]?.sheetQuantity ?? 0

          return (
            <div key={field.id} className="flex items-start gap-2">
              <div className="flex-1 space-y-1">
                <select
                  {...register(`items.${index}.dtfModelId`)}
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <option value="">Selecionar modelo...</option>
                  {models.map(m => (
                    <option key={m.id} value={m.id}>
                      {m.name} ({m.sheetLabel})
                    </option>
                  ))}
                </select>
                {errors.items?.[index]?.dtfModelId && (
                  <p className="text-xs text-danger">{errors.items[index].dtfModelId?.message}</p>
                )}
              </div>

              <div className="w-24 space-y-1">
                <Input
                  type="number"
                  min="1"
                  step="1"
                  placeholder="Folhas"
                  {...register(`items.${index}.sheetQuantity`, {
                    valueAsNumber: true,
                  })}
                />
                {errors.items?.[index]?.sheetQuantity && (
                  <p className="text-xs text-danger">{errors.items[index].sheetQuantity?.message}</p>
                )}
              </div>

              {selectedModel && qty > 0 && (
                <div className="pt-2 text-xs text-muted-foreground whitespace-nowrap">
                  = {qty * selectedModel.stampsPerSheet} est.
                </div>
              )}

              <button
                type="button"
                onClick={() => remove(index)}
                className="pt-2 text-xs text-danger hover:underline"
              >
                Remover
              </button>
            </div>
          )
        })}

        {errors.items?.message && (
          <p className="text-xs text-danger">{errors.items.message}</p>
        )}

        <button
          type="button"
          onClick={() => append({ dtfModelId: '', sheetQuantity: 1 })}
          className="text-xs text-primary hover:underline"
        >
          + Adicionar modelo
        </button>
      </div>

      <Button type="submit" disabled={isLoading} className="w-full">
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
```

- [ ] **Step 3: Typecheck**

```
cd frontend
pnpm typecheck
```

Esperado: sem erros.

- [ ] **Step 4: Commit**

```
git add frontend/src/features/dtf/orders/components/DtfOrderForm.tsx
git commit -m "feat: DtfOrderForm com useFieldArray e preview de estampas"
```

---

### Task 7: Frontend — DtfOrderList e DtfOrderPage

**Files:**
- Create: `frontend/src/features/dtf/orders/components/DtfOrderList.tsx`
- Create: `frontend/src/features/dtf/orders/DtfOrderPage.tsx`

- [ ] **Step 1: Criar DtfOrderList.tsx**

```typescript
// frontend/src/features/dtf/orders/components/DtfOrderList.tsx
import type { DtfOrderDto } from '../types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Rascunho',
  Sent: 'Enviado',
  Received: 'Recebido',
}

const STATUS_VARIANTS: Record<string, 'default' | 'secondary' | 'outline'> = {
  Draft: 'outline',
  Sent: 'secondary',
  Received: 'default',
}

interface Props {
  orders: DtfOrderDto[]
  onEdit: (order: DtfOrderDto) => void
  onSend: (order: DtfOrderDto) => void
  onReceive: (order: DtfOrderDto) => void
  onCancel: (order: DtfOrderDto) => void
}

export function DtfOrderList({ orders, onEdit, onSend, onReceive, onCancel }: Props) {
  if (orders.length === 0) {
    return (
      <div className="py-12 text-center text-sm text-muted-foreground">
        Nenhum pedido DTF encontrado.
      </div>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border text-left text-muted-foreground">
            <th className="pb-2 pr-4 font-medium">Nº</th>
            <th className="pb-2 pr-4 font-medium">Data</th>
            <th className="pb-2 pr-4 font-medium">Modelos</th>
            <th className="pb-2 pr-4 font-medium">Total de folhas</th>
            <th className="pb-2 pr-4 font-medium">Status</th>
            <th className="pb-2 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(order => {
            const totalSheets = order.items.reduce((acc, i) => acc + i.sheetQuantity, 0)
            const modelsSummary = order.items.map(i => i.modelName).join(', ')

            return (
              <tr key={order.id} className="border-b border-border/50 hover:bg-muted/30">
                <td className="py-3 pr-4 font-medium">#{order.orderNumber}</td>
                <td className="py-3 pr-4 text-muted-foreground">
                  {new Date(order.createdAt).toLocaleDateString('pt-BR')}
                </td>
                <td className="py-3 pr-4 max-w-[200px] truncate" title={modelsSummary}>
                  {modelsSummary || '—'}
                </td>
                <td className="py-3 pr-4">{totalSheets}</td>
                <td className="py-3 pr-4">
                  <Badge variant={STATUS_VARIANTS[order.status]}>
                    {STATUS_LABELS[order.status]}
                  </Badge>
                </td>
                <td className="py-3">
                  <div className="flex items-center gap-2">
                    {order.status === 'Draft' && (
                      <>
                        <Button size="sm" variant="outline" onClick={() => onEdit(order)}>
                          Editar
                        </Button>
                        <Button size="sm" variant="outline" onClick={() => onSend(order)}>
                          Marcar Enviado
                        </Button>
                        <Button size="sm" variant="outline" className="text-danger hover:text-danger" onClick={() => onCancel(order)}>
                          Cancelar
                        </Button>
                      </>
                    )}
                    {order.status === 'Sent' && (
                      <>
                        <Button size="sm" variant="outline" onClick={() => onReceive(order)}>
                          Marcar Recebido
                        </Button>
                        <Button size="sm" variant="outline" className="text-danger hover:text-danger" onClick={() => onCancel(order)}>
                          Cancelar
                        </Button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
```

- [ ] **Step 2: Criar DtfOrderPage.tsx**

Verificar o hook de modelos DTF disponível. O mais provável é que exista em `frontend/src/features/settings/dtf/hooks/`. Adaptar o import conforme necessário.

```typescript
// frontend/src/features/dtf/orders/DtfOrderPage.tsx
import { useState } from 'react'
import { useDtfOrders } from './hooks/useDtfOrders'
import { useCreateDtfOrder } from './hooks/useCreateDtfOrder'
import { useUpdateDtfOrder } from './hooks/useUpdateDtfOrder'
import { useSendDtfOrder } from './hooks/useSendDtfOrder'
import { useReceiveDtfOrder } from './hooks/useReceiveDtfOrder'
import { useCancelDtfOrder } from './hooks/useCancelDtfOrder'
import { DtfOrderList } from './components/DtfOrderList'
import { DtfOrderForm } from './components/DtfOrderForm'
import type { DtfOrderDto } from './types'
import type { DtfOrderFormData } from './schemas/dtfOrderSchema'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

interface DtfModelOption {
  id: string
  name: string
  sheetLabel: string
  stampsPerSheet: number
}

function useDtfModelOptions() {
  return useQuery({
    queryKey: ['dtf-models'],
    queryFn: async () => {
      const { data } = await api.get<DtfModelOption[]>('/dtf-models')
      return data
    },
  })
}

type Dialog =
  | { type: 'create' }
  | { type: 'edit'; order: DtfOrderDto }
  | { type: 'send'; order: DtfOrderDto }
  | { type: 'receive'; order: DtfOrderDto }
  | { type: 'cancel'; order: DtfOrderDto }
  | null

export function DtfOrderPage() {
  const [dialog, setDialog] = useState<Dialog>(null)

  const { data: orders = [], isLoading } = useDtfOrders()
  const { data: models = [] } = useDtfModelOptions()

  const createMutation = useCreateDtfOrder()
  const updateMutation = useUpdateDtfOrder()
  const sendMutation = useSendDtfOrder()
  const receiveMutation = useReceiveDtfOrder()
  const cancelMutation = useCancelDtfOrder()

  function handleSubmitCreate(data: DtfOrderFormData) {
    createMutation.mutate(data, { onSuccess: () => setDialog(null) })
  }

  function handleSubmitEdit(data: DtfOrderFormData) {
    if (dialog?.type !== 'edit') return
    updateMutation.mutate(
      { id: dialog.order.id, data },
      { onSuccess: () => setDialog(null) }
    )
  }

  function handleConfirmSend() {
    if (dialog?.type !== 'send') return
    sendMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  function handleConfirmReceive() {
    if (dialog?.type !== 'receive') return
    receiveMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  function handleConfirmCancel() {
    if (dialog?.type !== 'cancel') return
    cancelMutation.mutate(dialog.order.id, { onSuccess: () => setDialog(null) })
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Pedidos DTF</h1>
        <Button onClick={() => setDialog({ type: 'create' })}>+ Novo pedido</Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12">
          <div className="size-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        </div>
      ) : (
        <DtfOrderList
          orders={orders}
          onEdit={order => setDialog({ type: 'edit', order })}
          onSend={order => setDialog({ type: 'send', order })}
          onReceive={order => setDialog({ type: 'receive', order })}
          onCancel={order => setDialog({ type: 'cancel', order })}
        />
      )}

      {/* Dialog criação */}
      <Dialog open={dialog?.type === 'create'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Novo Pedido DTF</DialogTitle>
          </DialogHeader>
          <DtfOrderForm
            models={models}
            isLoading={createMutation.isPending}
            onSubmit={handleSubmitCreate}
          />
        </DialogContent>
      </Dialog>

      {/* Dialog edição */}
      <Dialog open={dialog?.type === 'edit'} onOpenChange={open => !open && setDialog(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              Editar Pedido DTF #{dialog?.type === 'edit' ? dialog.order.orderNumber : ''}
            </DialogTitle>
          </DialogHeader>
          {dialog?.type === 'edit' && (
            <DtfOrderForm
              models={models}
              defaultValues={dialog.order}
              isLoading={updateMutation.isPending}
              onSubmit={handleSubmitEdit}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Confirmação envio */}
      <AlertDialog open={dialog?.type === 'send'} onOpenChange={open => !open && setDialog(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Marcar como enviado?</AlertDialogTitle>
            <AlertDialogDescription>
              {dialog?.type === 'send' && `Marcar pedido #${dialog.order.orderNumber} como enviado ao fornecedor?`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmSend} disabled={sendMutation.isPending}>
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Confirmação recebimento */}
      <AlertDialog open={dialog?.type === 'receive'} onOpenChange={open => !open && setDialog(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Marcar como recebido?</AlertDialogTitle>
            <AlertDialogDescription>
              {dialog?.type === 'receive' && (
                <>
                  Marcar pedido #{dialog.order.orderNumber} como recebido?{' '}
                  O estoque DTF será atualizado automaticamente com{' '}
                  {dialog.order.items.reduce((acc, i) => acc + i.stampsTotal, 0)} estampas totais.
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmReceive} disabled={receiveMutation.isPending}>
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Confirmação cancelamento */}
      <AlertDialog open={dialog?.type === 'cancel'} onOpenChange={open => !open && setDialog(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancelar pedido?</AlertDialogTitle>
            <AlertDialogDescription>
              {dialog?.type === 'cancel' && `Cancelar pedido #${dialog.order.orderNumber}? Esta ação não pode ser desfeita.`}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Voltar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmCancel}
              disabled={cancelMutation.isPending}
              className="bg-danger hover:bg-danger/90"
            >
              Cancelar pedido
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
```

- [ ] **Step 3: Typecheck e lint**

```
cd frontend
pnpm typecheck && pnpm lint
```

Esperado: sem erros.

- [ ] **Step 4: Commit**

```
git add frontend/src/features/dtf/orders/
git commit -m "feat: DtfOrderList e DtfOrderPage com dialogs de criação, envio, recebimento e cancelamento"
```

---

### Task 8: Navegação — nav.ts e App.tsx

**Files:**
- Modify: `frontend/src/components/layout/nav.ts`
- Modify: `frontend/src/App.tsx`

- [ ] **Step 1: Adicionar tab dtf-orders em nav.ts**

No arquivo `frontend/src/components/layout/nav.ts`:

1. Adicionar `'dtf-orders'` ao tipo `TabId`:

```typescript
export type TabId =
  | 'fabric' | 'rolls' | 'cutting' | 'dtf-models' | 'dtf-orders'
  | 'dtf-stock' | 'shirt-stock' | 'supply-stock' | 'separation' | 'financial' | 'config'
  | 'supply-types' | 'sewing-orders' | 'sewers'
```

2. Adicionar import do ícone `FileText` do lucide-react:

```typescript
import {
  Boxes, Layers, Scissors, ClipboardList, Wallet, Shirt, Image, Settings, Archive, Package, Tag,
  Spool, Users, FileText,
  type LucideIcon,
} from 'lucide-react'
```

3. Adicionar item no grupo Produção, entre `sewing-orders` e `separation`:

```typescript
{ id: 'dtf-orders', label: 'Pedidos DTF', icon: FileText },
```

O grupo Produção completo fica:

```typescript
{
  label: 'Produção',
  items: [
    { id: 'rolls', label: 'Bobinas', icon: Layers },
    { id: 'cutting', label: 'Pedidos de Corte', icon: Scissors },
    { id: 'sewing-orders', label: 'Pedidos de Costura', icon: Spool },
    { id: 'dtf-orders', label: 'Pedidos DTF', icon: FileText },
    { id: 'separation', label: 'Lista de Separação', icon: ClipboardList },
  ],
},
```

- [ ] **Step 2: Atualizar App.tsx**

1. Adicionar import:

```typescript
import { DtfOrderPage } from '@/features/dtf/orders/DtfOrderPage'
```

2. Adicionar no record `pages`:

```typescript
'dtf-orders': <DtfOrderPage />,
```

- [ ] **Step 3: Typecheck e lint**

```
cd frontend
pnpm typecheck && pnpm lint
```

Esperado: sem erros.

- [ ] **Step 4: Commit**

```
git add frontend/src/components/layout/nav.ts frontend/src/App.tsx
git commit -m "feat: adicionar Pedidos DTF ao sidebar e App.tsx"
```

---

## Self-Review

### Spec coverage

| Requisito da spec | Task |
|---|---|
| DtfOrderStatus enum (Draft/Sent/Received) | Task 1 |
| DtfOrder aggregate com todos os métodos | Task 1 |
| DtfOrderItem com SheetQuantity | Task 1 |
| OrderNumber via MAX+1 | Task 2 (CreateDtfOrderCommandHandler) |
| DTOs DtfOrderDto e DtfOrderItemDto | Task 2 |
| CreateDtfOrderCommand | Task 2 |
| UpdateDtfOrderCommand | Task 2 |
| SendDtfOrderCommand | Task 2 |
| ReceiveDtfOrderCommand com criação de DtfStockMovements | Task 2 |
| CancelDtfOrderCommand | Task 2 |
| GetDtfOrdersQuery com filtro de status | Task 2 |
| DtfOrderConfiguration com Status como string | Task 3 |
| Migration AddDtfOrders | Task 3 |
| DtfOrdersController com 6 endpoints | Task 4 |
| TestApplicationDbContext atualizado | Task 4 |
| types.ts frontend | Task 5 |
| dtfOrderSchema Zod | Task 5 |
| 6 hooks TanStack Query | Task 5 |
| DtfOrderForm com useFieldArray | Task 6 |
| Preview N folhas × M estampas = X | Task 6 |
| DtfOrderList com ações por status | Task 7 |
| DtfOrderPage com todos os dialogs | Task 7 |
| Tab dtf-orders em Produção (sidebar) | Task 8 |

### Tipos consistentes

- `DtfOrder.Create(int orderNumber, List<(Guid DtfModelId, int SheetQuantity)> items, string? notes)` — usado em Task 1 (domain) e Task 2 (CreateDtfOrderCommandHandler): ✅
- `DtfOrderItem.UpdateSheetQuantity(int)` — definido em Task 1, usado em Task 2 (UpdateDtfOrderCommandHandler): ✅
- `DtfOrderItemDto.StampsTotal` = `SheetQuantity * StampsPerSheet` — calculado na query handler em Task 2, consumido em DtfOrderPage (dialog de recebimento): ✅
- `AddMovement(DtfMovementType.Entrada, stamps, reason, sheetCount)` — pattern verificado no `RegisterDtfMovementCommandHandler` existente: ✅

### Notas de implementação

- **ReceiveDtfOrderCommandHandler**: O pattern de criação de DtfStockItem quando não existente é extraído diretamente de `RegisterDtfMovementCommandHandler.cs` existente — mesmo padrão.
- **UpdateDtfOrderCommandHandler**: Usa merge de itens (adiciona novos, remove os que saíram, atualiza quantidades dos que permanecem). Isso requer que `DtfOrderItem` exponha `DtfModelId` publicamente (já está definido com `public` getter).
- **DtfOrderConfiguration**: `OnDelete(DeleteBehavior.SetNull)` para `DtfModelId` em `DtfOrderItemConfiguration` — se um modelo DTF for deletado, o item do pedido mantém o ID como null. O query handler trata isso com `model?.Name ?? "Modelo removido"`.
- **useDtfModelOptions em DtfOrderPage**: Faz query direta via `api.get('/dtf-models')` em vez de usar hook existente para evitar dependência cruzada de features. Se o hook `useDtfModels` existir em `features/settings/dtf/hooks/`, pode ser importado.
