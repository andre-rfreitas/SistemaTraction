# Cancelamento Manual com Estorno Automático — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir cancelamento manual de pedidos de corte em qualquer status, disparando automaticamente o estorno financeiro em cascata para pedidos `Delivered` e `SewingDelivered`.

**Architecture:** `CancelCuttingOrderCommandHandler` recebe `ISender` (MediatR) e, quando o pedido está `Delivered` ou `SewingDelivered`, busca o lançamento financeiro vinculado e despacha `ReverseFinancialEntryCommand` — reutilizando toda a lógica de cascata existente. Para Draft/SentToCutter mantém o comportamento atual. No frontend, o botão "Cancelar" passa a aparecer para todos os status não-cancelados, e o dialog de confirmação exibe texto contextual por status.

**Tech Stack:** .NET 9 / C# / MediatR / EF Core InMemory (testes) / NSubstitute / xUnit / FluentAssertions / React 18 / TypeScript / TanStack Query

---

## Arquivos modificados

**Backend:**
- Modify: `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs`
- Create: `backend/tests/Application.Tests/Cutting/CancelCuttingOrderCommandHandlerTests.cs`

**Frontend:**
- Modify: `frontend/src/features/cutting/orders/components/CuttingOrderList.tsx` (linha 58)
- Modify: `frontend/src/features/cutting/orders/CuttingOrderPage.tsx` (linhas 179-182)
- Modify: `frontend/src/features/cutting/orders/hooks/useCancelCuttingOrder.ts`

---

### Task 1: Backend — Extend CancelCuttingOrderCommandHandler (TDD)

**Files:**
- Create: `backend/tests/Application.Tests/Cutting/CancelCuttingOrderCommandHandlerTests.cs`
- Modify: `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs`

**Contexto do domínio:**
- `CuttingOrderStatus`: Draft, SentToCutter, Delivered, SewingDelivered, Cancelled
- `CuttingOrder.Cancel()` → lança DomainException para Delivered/SewingDelivered
- `CuttingOrder.CancelDelivered()` → cancela sem guards (para casos sem lançamento financeiro)
- `ReverseFinancialEntryCommand(Guid Id)` → despacha estorno, cascata cancela a ordem e reverte stock
- `FinancialEntry` tem: `ReferenceType (string?)`, `ReferenceId (Guid?)`, `IsReversal (bool)`
- `CuttingDelivery` tem `CuttingOrderId`; `SewingDelivery` tem `CuttingOrderId` e `IsDeleted`

- [ ] **Step 1: Criar o arquivo de testes com seed helpers e 4 testes failing**

Criar `backend/tests/Application.Tests/Cutting/CancelCuttingOrderCommandHandlerTests.cs`:

```csharp
using FluentAssertions;
using MediatR;
using NSubstitute;
using SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Tests.Cutting;

public class CancelCuttingOrderCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly CancelCuttingOrderCommandHandler _handler;

    public CancelCuttingOrderCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _sender = Substitute.For<ISender>();
        _handler = new CancelCuttingOrderCommandHandler(_context, _sender);
    }

    private async Task<(CuttingOrder order, CuttingDelivery delivery)> SeedDeliveredOrder()
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
        var delivery = CuttingDelivery.Create(order.Id, new Dictionary<string, int> { ["P"] = 5 }, 5m);
        _context.CuttingDeliveries.Add(delivery);
        order.MarkDelivered();
        roll.MarkConsumed();
        await _context.SaveChangesAsync();

        return (order, delivery);
    }

    private async Task<(CuttingOrder order, SewingDelivery delivery, Guid stockItemId)> SeedSewingDeliveredOrder()
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

        var sewingDelivery = SewingDelivery.Create(order.Id,
            new Dictionary<string, int> { ["P"] = 9 },
            new Dictionary<string, int> { ["P"] = 1 },
            50.4m, 27m);
        _context.SewingDeliveries.Add(sewingDelivery);

        var stockItem = StockItem.Create(fabricColor.Id, "Preto", "Malha", "Regular", "P", 9);
        _context.StockItems.Add(stockItem);
        _context.ShirtStockMovements.Add(ShirtStockMovement.Create(
            stockItem.Id, fabricColor.Id, "Preto", "P", 9,
            "Costura pedido #2", "Costureiro", order.Id));

        order.MarkSewingDelivered();
        await _context.SaveChangesAsync();

        return (order, sewingDelivery, stockItem.Id);
    }

    [Fact]
    public async Task Handle_Delivered_WithFinancialEntry_DispatchesReverseCommand()
    {
        var (order, delivery) = await SeedDeliveredOrder();
        var entry = FinancialEntry.CreateExpense("Corte", 5m, "Corte #1", delivery.Id, "CuttingDelivery");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<ReverseFinancialEntryCommand>(c => c.Id == entry.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Delivered_WithoutFinancialEntry_CancelsDirectly()
    {
        var (order, _) = await SeedDeliveredOrder();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<ReverseFinancialEntryCommand>(),
            Arg.Any<CancellationToken>());
        var updated = await _context.CuttingOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_SewingDelivered_WithFinancialEntry_DispatchesReverseCommand()
    {
        var (order, sewingDelivery, _) = await SeedSewingDeliveredOrder();
        var entry = FinancialEntry.CreateExpense("Costura", 50.4m, "Costura #2", sewingDelivery.Id, "SewingDelivery");
        _context.FinancialEntries.Add(entry);
        await _context.SaveChangesAsync();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<ReverseFinancialEntryCommand>(c => c.Id == entry.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SewingDelivered_WithoutFinancialEntry_CancelsDirectly()
    {
        var (order, _, _) = await SeedSewingDeliveredOrder();

        await _handler.Handle(new CancelCuttingOrderCommand(order.Id), CancellationToken.None);

        await _sender.DidNotReceive().Send(
            Arg.Any<ReverseFinancialEntryCommand>(),
            Arg.Any<CancellationToken>());
        var updated = await _context.CuttingOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(CuttingOrderStatus.Cancelled);
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Rodar os testes para confirmar que falham (handler ainda não compilará)**

```
cd backend && dotnet test --filter "CancelCuttingOrderCommandHandlerTests" --verbosity normal
```

Expected: falha de compilação — `CancelCuttingOrderCommandHandler` não recebe `ISender`.

- [ ] **Step 3: Reescrever o handler com suporte a ISender e lógica de cascata**

Substituir o conteúdo de `backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;

public class CancelCuttingOrderCommandHandler(IApplicationDbContext context, ISender sender)
    : IRequestHandler<CancelCuttingOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        if (order.Status == CuttingOrderStatus.Delivered)
        {
            await CancelWithFinancialReversal(order, "CuttingDelivery", cancellationToken);
            return Unit.Value;
        }

        if (order.Status == CuttingOrderStatus.SewingDelivered)
        {
            await CancelWithFinancialReversal(order, "SewingDelivery", cancellationToken);
            return Unit.Value;
        }

        if (order.Status == CuttingOrderStatus.SentToCutter)
        {
            foreach (var item in order.Items)
                item.FabricRoll?.RevertToAvailable();
        }

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task CancelWithFinancialReversal(CuttingOrder order, string referenceType, CancellationToken cancellationToken)
    {
        Guid? deliveryId = referenceType == "CuttingDelivery"
            ? (await context.CuttingDeliveries
                .FirstOrDefaultAsync(d => d.CuttingOrderId == order.Id, cancellationToken))?.Id
            : (await context.SewingDeliveries
                .FirstOrDefaultAsync(s => s.CuttingOrderId == order.Id && !s.IsDeleted, cancellationToken))?.Id;

        if (deliveryId is not null)
        {
            var entry = await context.FinancialEntries
                .FirstOrDefaultAsync(e => e.ReferenceType == referenceType
                                       && e.ReferenceId == deliveryId
                                       && !e.IsReversal, cancellationToken);

            if (entry is not null)
            {
                await sender.Send(new ReverseFinancialEntryCommand(entry.Id), cancellationToken);
                return;
            }
        }

        order.CancelDelivered();
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Rodar os testes do handler**

```
cd backend && dotnet test --filter "CancelCuttingOrderCommandHandlerTests" --verbosity normal
```

Expected: 4/4 PASS

- [ ] **Step 5: Rodar todos os testes do backend**

```
cd backend && dotnet test --verbosity normal
```

Expected: todos passam, 0 falhas. (Serão ≥117 testes — os 113 anteriores + os 4 novos.)

- [ ] **Step 6: Commit**

```
git add backend/tests/Application.Tests/Cutting/CancelCuttingOrderCommandHandlerTests.cs
git add backend/src/Application/Cutting/Commands/CancelCuttingOrder/CancelCuttingOrderCommandHandler.cs
git commit -m "feat: cancelamento manual dispara estorno financeiro em cascata para pedidos entregues"
```

---

### Task 2: Frontend — Botão + dialog para todos os status

**Files:**
- Modify: `frontend/src/features/cutting/orders/components/CuttingOrderList.tsx`
- Modify: `frontend/src/features/cutting/orders/CuttingOrderPage.tsx`
- Modify: `frontend/src/features/cutting/orders/hooks/useCancelCuttingOrder.ts`

**Contexto:**
- `CuttingOrderList.tsx` linha 58: `const canCancel = !isCancelled && (o.status === 'Draft' || o.status === 'SentToCutter')`
- `CuttingOrderPage.tsx` linhas 178-182: dialog com texto ternário simples para SentToCutter
- `useCancelCuttingOrder.ts`: `onSuccess` invalida apenas `cutting-orders` e `fabric-rolls`

- [ ] **Step 1: Atualizar `canCancel` em CuttingOrderList.tsx**

Localizar linha 58 e substituir:

```ts
// antes
const canCancel = !isCancelled && (o.status === 'Draft' || o.status === 'SentToCutter')

// depois
const canCancel = !isCancelled
```

- [ ] **Step 2: Atualizar o texto do dialog de confirmação em CuttingOrderPage.tsx**

Localizar o bloco de texto dentro do "Dialog: confirmar cancelamento" (linhas ~179-182) e substituir:

```tsx
// antes
<p className="text-sm text-muted-foreground">
  {cancelOrder?.status === 'SentToCutter'
    ? 'O pedido foi enviado ao cortador. As bobinas serão revertidas para disponível.'
    : 'O rascunho será removido permanentemente.'}
</p>

// depois
<p className="text-sm text-muted-foreground">
  {cancelOrder?.status === 'Delivered'
    ? 'Este cancelamento estornará o lançamento financeiro de corte e reverterá as bobinas de tecido para disponível.'
    : cancelOrder?.status === 'SewingDelivered'
    ? 'Este cancelamento estornará os lançamentos de costura e defeitos, reverterá o estoque de camisetas e cancelará o pedido.'
    : cancelOrder?.status === 'SentToCutter'
    ? 'O pedido foi enviado ao cortador. As bobinas serão revertidas para disponível.'
    : 'O rascunho será removido permanentemente.'}
</p>
```

- [ ] **Step 3: Adicionar invalidações em useCancelCuttingOrder.ts**

Substituir o conteúdo do arquivo:

```ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useCancelCuttingOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (orderId: string) => {
      await api.delete(`/cutting-orders/${orderId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cutting-orders'] })
      queryClient.invalidateQueries({ queryKey: ['fabric-rolls'] })
      queryClient.invalidateQueries({ queryKey: ['financial-entries'] })
      queryClient.invalidateQueries({ queryKey: ['financial-summary'] })
      queryClient.invalidateQueries({ queryKey: ['shirt-stock'] })
    },
  })
}
```

- [ ] **Step 4: Rodar typecheck e lint**

```
cd frontend && pnpm typecheck && pnpm lint
```

Expected: 0 errors, 0 warnings novos.

- [ ] **Step 5: Commit**

```
git add frontend/src/features/cutting/orders/components/CuttingOrderList.tsx
git add frontend/src/features/cutting/orders/CuttingOrderPage.tsx
git add frontend/src/features/cutting/orders/hooks/useCancelCuttingOrder.ts
git commit -m "feat: exibir botão cancelar para todos status e texto contextual no dialog"
```

---

## Verificação final

Após as duas tasks:

```
cd backend && dotnet test --verbosity normal
```

Expected: todos os testes passam.

Cenários manuais para validar:
1. Pedido `Delivered` → clicar Cancelar → dialog mostra aviso de estorno de corte → confirmar → pedido fica Cancelado na lista, bobinas voltam para Disponível, lançamento financeiro ganha estorno na tela financeira
2. Pedido `SewingDelivered` → clicar Cancelar → dialog mostra aviso de costura+defeitos+estoque → confirmar → pedido fica Cancelado, estoque de camisetas revertido, dois estornos criados no financeiro
3. Pedido `Draft` → cancelar → comportamento anterior mantido (sem mudança)
4. Pedido `SentToCutter` → cancelar → comportamento anterior mantido (bobinas revertidas)
