# Setup Inicial + Módulo 1A (FabricType + FabricColor) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Criar o monorepo SistemaTraction do zero com backend .NET 9 Clean Architecture + frontend React 18 TypeScript, banco SQL Server LocalDB, e implementar o CRUD completo de FabricType e FabricColor com endpoints REST, migrations, seed e testes.

**Architecture:** Backend em Clean Architecture com 4 camadas (Domain / Application / Infrastructure / API) usando CQRS via MediatR. Frontend React com TanStack Query para data fetching, React Hook Form + Zod para formulários, Tailwind + shadcn/ui para UI. Banco SQL Server LocalDB em dev.

**Tech Stack:** .NET 9, C#, EF Core, MediatR, FluentValidation, Serilog, xUnit, FluentAssertions / React 18, TypeScript strict, Vite, TanStack Query, React Hook Form, Zod, Tailwind CSS, shadcn/ui, pnpm, Vitest

**Notas de ambiente:**
- .NET 9 instalado (não .NET 8 — spec compatível)
- pnpm não instalado — será instalado na Task 1
- Docker não disponível — usar SQL Server LocalDB
- Projeto root: `C:\Users\andre\OneDrive\Desktop\Sistema Traction\`

---

## Mapa de arquivos

### Backend
```
backend/
├── SistemaTraction.sln
├── src/
│   ├── Domain/
│   │   ├── SistemaTraction.Domain.csproj
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs          # Id, CreatedAt, UpdatedAt, IsDeleted
│   │   │   └── DomainException.cs
│   │   └── Fabric/
│   │       ├── FabricType.cs          # aggregate root
│   │       └── FabricColor.cs         # child entity
│   ├── Application/
│   │   ├── SistemaTraction.Application.csproj
│   │   ├── Common/
│   │   │   └── Interfaces/
│   │   │       └── IApplicationDbContext.cs
│   │   └── Fabric/
│   │       ├── Commands/
│   │       │   ├── CreateFabricType/
│   │       │   │   ├── CreateFabricTypeCommand.cs
│   │       │   │   ├── CreateFabricTypeCommandHandler.cs
│   │       │   │   └── CreateFabricTypeCommandValidator.cs
│   │       │   ├── UpdateFabricType/
│   │       │   │   ├── UpdateFabricTypeCommand.cs
│   │       │   │   ├── UpdateFabricTypeCommandHandler.cs
│   │       │   │   └── UpdateFabricTypeCommandValidator.cs
│   │       │   ├── DeleteFabricType/
│   │       │   │   ├── DeleteFabricTypeCommand.cs
│   │       │   │   └── DeleteFabricTypeCommandHandler.cs
│   │       │   ├── CreateFabricColor/
│   │       │   │   ├── CreateFabricColorCommand.cs
│   │       │   │   ├── CreateFabricColorCommandHandler.cs
│   │       │   │   └── CreateFabricColorCommandValidator.cs
│   │       │   ├── UpdateFabricColor/
│   │       │   │   ├── UpdateFabricColorCommand.cs
│   │       │   │   ├── UpdateFabricColorCommandHandler.cs
│   │       │   │   └── UpdateFabricColorCommandValidator.cs
│   │       │   └── DeleteFabricColor/
│   │       │       ├── DeleteFabricColorCommand.cs
│   │       │       └── DeleteFabricColorCommandHandler.cs
│   │       ├── Queries/
│   │       │   ├── GetFabricTypes/
│   │       │   │   ├── GetFabricTypesQuery.cs
│   │       │   │   └── GetFabricTypesQueryHandler.cs
│   │       │   └── GetFabricTypeById/
│   │       │       ├── GetFabricTypeByIdQuery.cs
│   │       │       └── GetFabricTypeByIdQueryHandler.cs
│   │       └── DTOs/
│   │           ├── FabricTypeDto.cs
│   │           └── FabricColorDto.cs
│   ├── Infrastructure/
│   │   ├── SistemaTraction.Infrastructure.csproj
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── FabricTypeConfiguration.cs
│   │   │   │   └── FabricColorConfiguration.cs
│   │   │   └── Migrations/           # gerado pelo EF
│   │   └── DependencyInjection.cs
│   └── API/
│       ├── SistemaTraction.API.csproj
│       ├── Program.cs
│       ├── Controllers/
│       │   └── FabricTypesController.cs
│       └── DependencyInjection.cs
└── tests/
    └── Application.Tests/
        ├── SistemaTraction.Application.Tests.csproj
        └── Fabric/
            ├── CreateFabricTypeCommandHandlerTests.cs
            ├── UpdateFabricTypeCommandHandlerTests.cs
            ├── DeleteFabricTypeCommandHandlerTests.cs
            ├── CreateFabricColorCommandHandlerTests.cs
            └── GetFabricTypesQueryHandlerTests.cs
```

### Frontend
```
frontend/
├── package.json
├── pnpm-lock.yaml
├── vite.config.ts
├── tsconfig.json
├── tsconfig.app.json
├── index.html
├── tailwind.config.ts
├── components.json              # shadcn/ui config
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── lib/
    │   ├── api.ts               # axios base client
    │   └── queryClient.ts
    ├── components/
    │   └── ui/                  # shadcn/ui components (gerado)
    └── features/
        └── settings/
            └── fabric/
                ├── hooks/
                │   ├── useFabricTypes.ts
                │   ├── useCreateFabricType.ts
                │   ├── useUpdateFabricType.ts
                │   ├── useDeleteFabricType.ts
                │   ├── useCreateFabricColor.ts
                │   ├── useUpdateFabricColor.ts
                │   └── useDeleteFabricColor.ts
                ├── schemas/
                │   └── fabricTypeSchema.ts
                ├── components/
                │   ├── FabricTypeList.tsx
                │   ├── FabricTypeForm.tsx
                │   └── FabricColorForm.tsx
                └── FabricTypePage.tsx
```

---

## Task 1: Instalar pnpm e verificar ambiente

**Files:** nenhum arquivo de projeto criado

- [ ] **Step 1: Instalar pnpm globalmente**

```bash
npm install -g pnpm
```

- [ ] **Step 2: Verificar instalação**

```bash
pnpm --version
dotnet --version
```

Expected: pnpm >= 9.x, dotnet 9.0.x

---

## Task 2: Criar estrutura do monorepo e solução .NET

**Files:**
- Create: `backend/SistemaTraction.sln`
- Create: `backend/src/Domain/SistemaTraction.Domain.csproj`
- Create: `backend/src/Application/SistemaTraction.Application.csproj`
- Create: `backend/src/Infrastructure/SistemaTraction.Infrastructure.csproj`
- Create: `backend/src/API/SistemaTraction.API.csproj`
- Create: `backend/tests/Application.Tests/SistemaTraction.Application.Tests.csproj`

- [ ] **Step 1: Criar solução e projetos**

```bash
cd backend

dotnet new sln -n SistemaTraction

dotnet new classlib -n SistemaTraction.Domain -o src/Domain --framework net9.0
dotnet new classlib -n SistemaTraction.Application -o src/Application --framework net9.0
dotnet new classlib -n SistemaTraction.Infrastructure -o src/Infrastructure --framework net9.0
dotnet new webapi -n SistemaTraction.API -o src/API --framework net9.0 --use-controllers

dotnet new xunit -n SistemaTraction.Application.Tests -o tests/Application.Tests --framework net9.0
```

- [ ] **Step 2: Adicionar projetos à solução**

```bash
cd backend

dotnet sln add src/Domain/SistemaTraction.Domain.csproj
dotnet sln add src/Application/SistemaTraction.Application.csproj
dotnet sln add src/Infrastructure/SistemaTraction.Infrastructure.csproj
dotnet sln add src/API/SistemaTraction.API.csproj
dotnet sln add tests/Application.Tests/SistemaTraction.Application.Tests.csproj
```

- [ ] **Step 3: Adicionar referências entre projetos**

```bash
cd backend

# Application depende de Domain
dotnet add src/Application/SistemaTraction.Application.csproj reference src/Domain/SistemaTraction.Domain.csproj

# Infrastructure depende de Application e Domain
dotnet add src/Infrastructure/SistemaTraction.Infrastructure.csproj reference src/Application/SistemaTraction.Application.csproj
dotnet add src/Infrastructure/SistemaTraction.Infrastructure.csproj reference src/Domain/SistemaTraction.Domain.csproj

# API depende de Application e Infrastructure
dotnet add src/API/SistemaTraction.API.csproj reference src/Application/SistemaTraction.Application.csproj
dotnet add src/API/SistemaTraction.API.csproj reference src/Infrastructure/SistemaTraction.Infrastructure.csproj

# Tests dependem de Application e Domain
dotnet add tests/Application.Tests/SistemaTraction.Application.Tests.csproj reference src/Application/SistemaTraction.Application.csproj
dotnet add tests/Application.Tests/SistemaTraction.Application.Tests.csproj reference src/Domain/SistemaTraction.Domain.csproj
```

- [ ] **Step 4: Instalar pacotes NuGet**

```bash
cd backend

# Domain — sem dependências externas

# Application
dotnet add src/Application package MediatR --version 12.*
dotnet add src/Application package FluentValidation.DependencyInjectionExtensions --version 11.*
dotnet add src/Application package Microsoft.Extensions.DependencyInjection.Abstractions

# Infrastructure
dotnet add src/Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 9.*
dotnet add src/Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.*
dotnet add src/Infrastructure package Serilog.AspNetCore --version 8.*
dotnet add src/Infrastructure package Serilog.Sinks.Console

# API
dotnet add src/API package Microsoft.EntityFrameworkCore.Design --version 9.*
dotnet add src/API package Swashbuckle.AspNetCore

# Tests
dotnet add tests/Application.Tests package FluentAssertions --version 7.*
dotnet add tests/Application.Tests package NSubstitute --version 5.*
dotnet add tests/Application.Tests package Microsoft.EntityFrameworkCore.InMemory --version 9.*
```

- [ ] **Step 5: Remover arquivos de template desnecessários**

```bash
cd backend
rm -f src/Domain/Class1.cs
rm -f src/Application/Class1.cs
rm -f src/Infrastructure/Class1.cs
rm -f src/API/WeatherForecast.cs
rm -f src/API/Controllers/WeatherForecastController.cs
rm -f tests/Application.Tests/UnitTest1.cs
```

- [ ] **Step 6: Build para verificar que tudo compila**

```bash
cd backend
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 7: Commit**

```bash
git add backend/
git commit -m "chore: configurar solução .NET 9 com Clean Architecture"
```

---

## Task 3: Entidades de domínio (Domain layer)

**Files:**
- Create: `backend/src/Domain/Common/BaseEntity.cs`
- Create: `backend/src/Domain/Common/DomainException.cs`
- Create: `backend/src/Domain/Fabric/FabricType.cs`
- Create: `backend/src/Domain/Fabric/FabricColor.cs`

- [ ] **Step 1: Criar BaseEntity**

```csharp
// backend/src/Domain/Common/BaseEntity.cs
namespace SistemaTraction.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    public void MarkAsDeleted() => IsDeleted = true;
    public void TouchUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Step 2: Criar DomainException**

```csharp
// backend/src/Domain/Common/DomainException.cs
namespace SistemaTraction.Domain.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

- [ ] **Step 3: Criar FabricColor**

```csharp
// backend/src/Domain/Fabric/FabricColor.cs
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Fabric;

public class FabricColor : BaseEntity
{
    public Guid FabricTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? HexCode { get; private set; }

    // EF Core constructor
    private FabricColor() { }

    public static FabricColor Create(Guid fabricTypeId, string name, string? hexCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da cor não pode ser vazio.");

        return new FabricColor
        {
            FabricTypeId = fabricTypeId,
            Name = name.Trim(),
            HexCode = hexCode?.Trim()
        };
    }

    public void Update(string name, string? hexCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da cor não pode ser vazio.");

        Name = name.Trim();
        HexCode = hexCode?.Trim();
        TouchUpdatedAt();
    }
}
```

- [ ] **Step 4: Criar FabricType**

```csharp
// backend/src/Domain/Fabric/FabricType.cs
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Fabric;

public class FabricType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Variation { get; private set; } = string.Empty;
    public decimal PricePerKg { get; private set; }
    public decimal AverageKgPerRoll { get; private set; }
    public int? AveragePiecesPerRoll { get; private set; }

    private readonly List<FabricColor> _colors = [];
    public IReadOnlyCollection<FabricColor> Colors => _colors.AsReadOnly();

    // EF Core constructor
    private FabricType() { }

    public static FabricType Create(string name, string variation, decimal pricePerKg, decimal averageKgPerRoll, int? averagePiecesPerRoll = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de tecido não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(variation))
            throw new DomainException("Variação do tipo de tecido não pode ser vazia.");

        if (pricePerKg <= 0)
            throw new DomainException("Preço por kg deve ser maior que zero.");

        if (averageKgPerRoll <= 0)
            throw new DomainException("Média de kg por bobina deve ser maior que zero.");

        return new FabricType
        {
            Name = name.Trim(),
            Variation = variation.Trim(),
            PricePerKg = pricePerKg,
            AverageKgPerRoll = averageKgPerRoll,
            AveragePiecesPerRoll = averagePiecesPerRoll
        };
    }

    public void Update(string name, string variation, decimal pricePerKg, decimal averageKgPerRoll, int? averagePiecesPerRoll)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de tecido não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(variation))
            throw new DomainException("Variação do tipo de tecido não pode ser vazia.");

        if (pricePerKg <= 0)
            throw new DomainException("Preço por kg deve ser maior que zero.");

        if (averageKgPerRoll <= 0)
            throw new DomainException("Média de kg por bobina deve ser maior que zero.");

        Name = name.Trim();
        Variation = variation.Trim();
        PricePerKg = pricePerKg;
        AverageKgPerRoll = averageKgPerRoll;
        AveragePiecesPerRoll = averagePiecesPerRoll;
        TouchUpdatedAt();
    }

    public FabricColor AddColor(string name, string? hexCode = null)
    {
        if (_colors.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
            throw new DomainException($"Cor '{name}' já existe neste tipo de tecido.");

        var color = FabricColor.Create(Id, name, hexCode);
        _colors.Add(color);
        TouchUpdatedAt();
        return color;
    }

    public void RemoveColor(Guid colorId)
    {
        var color = _colors.FirstOrDefault(c => c.Id == colorId && !c.IsDeleted)
            ?? throw new DomainException("Cor não encontrada.");

        color.MarkAsDeleted();
        TouchUpdatedAt();
    }
}
```

- [ ] **Step 5: Build para verificar Domain**

```bash
cd backend
dotnet build src/Domain/SistemaTraction.Domain.csproj
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git add backend/src/Domain/
git commit -m "feat: adicionar entidades de domínio FabricType e FabricColor"
```

---

## Task 4: Interface do DbContext e DTOs (Application layer)

**Files:**
- Create: `backend/src/Application/Common/Interfaces/IApplicationDbContext.cs`
- Create: `backend/src/Application/Fabric/DTOs/FabricColorDto.cs`
- Create: `backend/src/Application/Fabric/DTOs/FabricTypeDto.cs`
- Create: `backend/src/Application/DependencyInjection.cs`

- [ ] **Step 1: Criar interface do DbContext**

```csharp
// backend/src/Application/Common/Interfaces/IApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Criar DTOs**

```csharp
// backend/src/Application/Fabric/DTOs/FabricColorDto.cs
namespace SistemaTraction.Application.Fabric.DTOs;

public record FabricColorDto(
    Guid Id,
    Guid FabricTypeId,
    string Name,
    string? HexCode
);
```

```csharp
// backend/src/Application/Fabric/DTOs/FabricTypeDto.cs
namespace SistemaTraction.Application.Fabric.DTOs;

public record FabricTypeDto(
    Guid Id,
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll,
    IReadOnlyCollection<FabricColorDto> Colors
);
```

- [ ] **Step 3: Criar DI da Application**

```csharp
// backend/src/Application/DependencyInjection.cs
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SistemaTraction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

- [ ] **Step 4: Build**

```bash
cd backend
dotnet build src/Application/SistemaTraction.Application.csproj
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 5: Commit**

```bash
git add backend/src/Application/
git commit -m "feat: adicionar interface IApplicationDbContext, DTOs e DI da Application"
```

---

## Task 5: Queries — GetFabricTypes e GetFabricTypeById

**Files:**
- Create: `backend/src/Application/Fabric/Queries/GetFabricTypes/GetFabricTypesQuery.cs`
- Create: `backend/src/Application/Fabric/Queries/GetFabricTypes/GetFabricTypesQueryHandler.cs`
- Create: `backend/src/Application/Fabric/Queries/GetFabricTypeById/GetFabricTypeByIdQuery.cs`
- Create: `backend/src/Application/Fabric/Queries/GetFabricTypeById/GetFabricTypeByIdQueryHandler.cs`

- [ ] **Step 1: Escrever testes primeiro (TDD)**

```csharp
// backend/tests/Application.Tests/Fabric/GetFabricTypesQueryHandlerTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypes;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Infrastructure.Persistence;

namespace SistemaTraction.Application.Tests.Fabric;

public class GetFabricTypesQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetFabricTypesQueryHandler _handler;

    public GetFabricTypesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new GetFabricTypesQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ReturnsAllNonDeletedFabricTypes()
    {
        // Arrange
        var active = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        var deleted = FabricType.Create("Moletom", "Premium", 35m, 8m, null);
        deleted.MarkAsDeleted();

        _context.FabricTypes.AddRange(active, deleted);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetFabricTypesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Malha");
    }

    [Fact]
    public async Task Handle_IncludesColorsInResult()
    {
        // Arrange
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto", "#000000");
        fabricType.AddColor("Branco", "#FFFFFF");

        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetFabricTypesQuery(), CancellationToken.None);

        // Assert
        result[0].Colors.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Rodar testes (devem falhar — handler não existe ainda)**

```bash
cd backend
dotnet test tests/Application.Tests/ --filter "GetFabricTypesQueryHandlerTests"
```

Expected: FAIL — tipo `GetFabricTypesQueryHandler` não encontrado

- [ ] **Step 3: Criar as queries**

```csharp
// backend/src/Application/Fabric/Queries/GetFabricTypes/GetFabricTypesQuery.cs
using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypes;

public record GetFabricTypesQuery : IRequest<List<FabricTypeDto>>;
```

```csharp
// backend/src/Application/Fabric/Queries/GetFabricTypes/GetFabricTypesQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypes;

public class GetFabricTypesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricTypesQuery, List<FabricTypeDto>>
{
    public async Task<List<FabricTypeDto>> Handle(GetFabricTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await context.FabricTypes
            .Include(t => t.Colors.Where(c => !c.IsDeleted))
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return types.Select(t => new FabricTypeDto(
            t.Id,
            t.Name,
            t.Variation,
            t.PricePerKg,
            t.AverageKgPerRoll,
            t.AveragePiecesPerRoll,
            t.Colors.Select(c => new FabricColorDto(c.Id, c.FabricTypeId, c.Name, c.HexCode)).ToList()
        )).ToList();
    }
}
```

```csharp
// backend/src/Application/Fabric/Queries/GetFabricTypeById/GetFabricTypeByIdQuery.cs
using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;

public record GetFabricTypeByIdQuery(Guid Id) : IRequest<FabricTypeDto?>;
```

```csharp
// backend/src/Application/Fabric/Queries/GetFabricTypeById/GetFabricTypeByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;

public class GetFabricTypeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricTypeByIdQuery, FabricTypeDto?>
{
    public async Task<FabricTypeDto?> Handle(GetFabricTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await context.FabricTypes
            .Include(t => t.Colors.Where(c => !c.IsDeleted))
            .Where(t => t.Id == request.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (t is null) return null;

        return new FabricTypeDto(
            t.Id,
            t.Name,
            t.Variation,
            t.PricePerKg,
            t.AverageKgPerRoll,
            t.AveragePiecesPerRoll,
            t.Colors.Select(c => new FabricColorDto(c.Id, c.FabricTypeId, c.Name, c.HexCode)).ToList()
        );
    }
}
```

- [ ] **Step 4: Rodar testes (devem passar)**

```bash
cd backend
dotnet test tests/Application.Tests/ --filter "GetFabricTypesQueryHandlerTests"
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add backend/src/Application/Fabric/Queries/ backend/tests/
git commit -m "feat: adicionar queries GetFabricTypes e GetFabricTypeById com testes"
```

---

## Task 6: Commands — CreateFabricType, UpdateFabricType, DeleteFabricType

**Files:**
- Create: `backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommandHandler.cs`
- Create: `backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommandValidator.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommandHandler.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommandValidator.cs`
- Create: `backend/src/Application/Fabric/Commands/DeleteFabricType/DeleteFabricTypeCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/DeleteFabricType/DeleteFabricTypeCommandHandler.cs`
- Create: `backend/tests/Application.Tests/Fabric/CreateFabricTypeCommandHandlerTests.cs`

- [ ] **Step 1: Escrever testes (TDD)**

```csharp
// backend/tests/Application.Tests/Fabric/CreateFabricTypeCommandHandlerTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Fabric.Commands.CreateFabricType;
using SistemaTraction.Infrastructure.Persistence;

namespace SistemaTraction.Application.Tests.Fabric;

public class CreateFabricTypeCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateFabricTypeCommandHandler _handler;

    public CreateFabricTypeCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new CreateFabricTypeCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFabricType()
    {
        // Arrange
        var command = new CreateFabricTypeCommand("Malha", "Regular", 20m, 10m, 80);

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _context.FabricTypes.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Malha");
        saved.Variation.Should().Be("Regular");
        saved.PricePerKg.Should().Be(20m);
    }

    [Fact]
    public async Task Handle_InvalidPrice_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateFabricTypeCommand("Malha", "Regular", 0m, 10m, null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SistemaTraction.Domain.Common.DomainException>()
            .WithMessage("*maior que zero*");
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Rodar testes para confirmar que falham**

```bash
cd backend
dotnet test tests/Application.Tests/ --filter "CreateFabricTypeCommandHandlerTests"
```

Expected: FAIL — tipo não encontrado

- [ ] **Step 3: Criar commands de FabricType**

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public record CreateFabricTypeCommand(
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll
) : IRequest<Guid>;
```

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommandHandler.cs
using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public class CreateFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFabricTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = FabricType.Create(
            request.Name,
            request.Variation,
            request.PricePerKg,
            request.AverageKgPerRoll,
            request.AveragePiecesPerRoll);

        context.FabricTypes.Add(fabricType);
        await context.SaveChangesAsync(cancellationToken);

        return fabricType.Id;
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricType/CreateFabricTypeCommandValidator.cs
using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public class CreateFabricTypeCommandValidator : AbstractValidator<CreateFabricTypeCommand>
{
    public CreateFabricTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Variation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerKg).GreaterThan(0);
        RuleFor(x => x.AverageKgPerRoll).GreaterThan(0);
        RuleFor(x => x.AveragePiecesPerRoll).GreaterThan(0).When(x => x.AveragePiecesPerRoll.HasValue);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public record UpdateFabricTypeCommand(
    Guid Id,
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll
) : IRequest;
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public class UpdateFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFabricTypeCommand>
{
    public async Task Handle(UpdateFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        fabricType.Update(request.Name, request.Variation, request.PricePerKg, request.AverageKgPerRoll, request.AveragePiecesPerRoll);

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricType/UpdateFabricTypeCommandValidator.cs
using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public class UpdateFabricTypeCommandValidator : AbstractValidator<UpdateFabricTypeCommand>
{
    public UpdateFabricTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Variation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerKg).GreaterThan(0);
        RuleFor(x => x.AverageKgPerRoll).GreaterThan(0);
        RuleFor(x => x.AveragePiecesPerRoll).GreaterThan(0).When(x => x.AveragePiecesPerRoll.HasValue);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/DeleteFabricType/DeleteFabricTypeCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricType;

public record DeleteFabricTypeCommand(Guid Id) : IRequest;
```

```csharp
// backend/src/Application/Fabric/Commands/DeleteFabricType/DeleteFabricTypeCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricType;

public class DeleteFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFabricTypeCommand>
{
    public async Task Handle(DeleteFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        fabricType.MarkAsDeleted();
        fabricType.TouchUpdatedAt();

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Rodar testes**

```bash
cd backend
dotnet test tests/Application.Tests/ --filter "CreateFabricTypeCommandHandlerTests"
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add backend/src/Application/Fabric/Commands/ backend/tests/
git commit -m "feat: adicionar commands CreateFabricType, UpdateFabricType, DeleteFabricType"
```

---

## Task 7: Commands — CreateFabricColor, UpdateFabricColor, DeleteFabricColor

**Files:**
- Create: `backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommandHandler.cs`
- Create: `backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommandValidator.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommandHandler.cs`
- Create: `backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommandValidator.cs`
- Create: `backend/src/Application/Fabric/Commands/DeleteFabricColor/DeleteFabricColorCommand.cs`
- Create: `backend/src/Application/Fabric/Commands/DeleteFabricColor/DeleteFabricColorCommandHandler.cs`
- Create: `backend/tests/Application.Tests/Fabric/CreateFabricColorCommandHandlerTests.cs`

- [ ] **Step 1: Escrever testes (TDD)**

```csharp
// backend/tests/Application.Tests/Fabric/CreateFabricColorCommandHandlerTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Fabric.Commands.CreateFabricColor;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Infrastructure.Persistence;

namespace SistemaTraction.Application.Tests.Fabric;

public class CreateFabricColorCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateFabricColorCommandHandler _handler;

    public CreateFabricColorCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new CreateFabricColorCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsColorToFabricType()
    {
        // Arrange
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var command = new CreateFabricColorCommand(fabricType.Id, "Preto", "#000000");

        // Act
        var colorId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedType = await _context.FabricTypes
            .Include(t => t.Colors)
            .FirstAsync(t => t.Id == fabricType.Id);

        savedType.Colors.Should().ContainSingle(c => c.Id == colorId && c.Name == "Preto");
    }

    [Fact]
    public async Task Handle_DuplicateColor_ThrowsDomainException()
    {
        // Arrange
        var fabricType = FabricType.Create("Malha", "Regular", 20m, 10m, 80);
        fabricType.AddColor("Preto");
        _context.FabricTypes.Add(fabricType);
        await _context.SaveChangesAsync();

        var command = new CreateFabricColorCommand(fabricType.Id, "Preto", null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Preto*");
    }

    [Fact]
    public async Task Handle_FabricTypeNotFound_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateFabricColorCommand(Guid.NewGuid(), "Preto", null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Rodar testes para confirmar que falham**

```bash
cd backend
dotnet test tests/Application.Tests/ --filter "CreateFabricColorCommandHandlerTests"
```

Expected: FAIL

- [ ] **Step 3: Criar commands de FabricColor**

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public record CreateFabricColorCommand(Guid FabricTypeId, string Name, string? HexCode) : IRequest<Guid>;
```

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public class CreateFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFabricColorCommand, Guid>
{
    public async Task<Guid> Handle(CreateFabricColorCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == request.FabricTypeId && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        var color = fabricType.AddColor(request.Name, request.HexCode);

        await context.SaveChangesAsync(cancellationToken);

        return color.Id;
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/CreateFabricColor/CreateFabricColorCommandValidator.cs
using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public class CreateFabricColorCommandValidator : AbstractValidator<CreateFabricColorCommand>
{
    public CreateFabricColorCommandValidator()
    {
        RuleFor(x => x.FabricTypeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HexCode).Matches("^#[0-9A-Fa-f]{6}$").When(x => x.HexCode is not null);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public record UpdateFabricColorCommand(Guid Id, string Name, string? HexCode) : IRequest;
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public class UpdateFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFabricColorCommand>
{
    public async Task Handle(UpdateFabricColorCommand request, CancellationToken cancellationToken)
    {
        var color = await context.FabricColors
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Cor não encontrada.");

        color.Update(request.Name, request.HexCode);

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/UpdateFabricColor/UpdateFabricColorCommandValidator.cs
using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public class UpdateFabricColorCommandValidator : AbstractValidator<UpdateFabricColorCommand>
{
    public UpdateFabricColorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HexCode).Matches("^#[0-9A-Fa-f]{6}$").When(x => x.HexCode is not null);
    }
}
```

```csharp
// backend/src/Application/Fabric/Commands/DeleteFabricColor/DeleteFabricColorCommand.cs
using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;

public record DeleteFabricColorCommand(Guid Id) : IRequest;
```

```csharp
// backend/src/Application/Fabric/Commands/DeleteFabricColor/DeleteFabricColorCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;

public class DeleteFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFabricColorCommand>
{
    public async Task Handle(DeleteFabricColorCommand request, CancellationToken cancellationToken)
    {
        var color = await context.FabricColors
            .Include(c => c.FabricType)
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Cor não encontrada.");

        color.FabricType!.RemoveColor(request.Id);

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

> **Nota:** Para o handler de `DeleteFabricColor` funcionar, `FabricColor` precisa ter navegação reversa para `FabricType`. Adicionar propriedade de navegação em `FabricColor`:
> ```csharp
> // backend/src/Domain/Fabric/FabricColor.cs — adicionar dentro da classe:
> public FabricType? FabricType { get; private set; }
> ```

- [ ] **Step 4: Rodar todos os testes**

```bash
cd backend
dotnet test tests/Application.Tests/
```

Expected: todos PASS

- [ ] **Step 5: Commit**

```bash
git add backend/src/Application/Fabric/Commands/ backend/tests/ backend/src/Domain/
git commit -m "feat: adicionar commands FabricColor (create, update, delete) com testes"
```

---

## Task 8: Infrastructure — DbContext, configurações EF e migrations

**Files:**
- Create: `backend/src/Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `backend/src/Infrastructure/Persistence/Configurations/FabricTypeConfiguration.cs`
- Create: `backend/src/Infrastructure/Persistence/Configurations/FabricColorConfiguration.cs`
- Create: `backend/src/Infrastructure/DependencyInjection.cs`
- Modify: `backend/src/API/appsettings.json` (connection string LocalDB)
- Modify: `backend/src/API/appsettings.Development.json` (não versionar)

- [ ] **Step 1: Criar ApplicationDbContext**

```csharp
// backend/src/Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 2: Criar configurações EF**

```csharp
// backend/src/Infrastructure/Persistence/Configurations/FabricTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FabricTypeConfiguration : IEntityTypeConfiguration<FabricType>
{
    public void Configure(EntityTypeBuilder<FabricType> builder)
    {
        builder.ToTable("FabricTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Variation).IsRequired().HasMaxLength(100);
        builder.Property(t => t.PricePerKg).HasPrecision(18, 2);
        builder.Property(t => t.AverageKgPerRoll).HasPrecision(18, 3);

        builder.HasIndex(t => new { t.Name, t.Variation }).IsUnique();

        builder.HasMany(t => t.Colors)
            .WithOne(c => c.FabricType)
            .HasForeignKey(c => c.FabricTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

```csharp
// backend/src/Infrastructure/Persistence/Configurations/FabricColorConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FabricColorConfiguration : IEntityTypeConfiguration<FabricColor>
{
    public void Configure(EntityTypeBuilder<FabricColor> builder)
    {
        builder.ToTable("FabricColors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.HexCode).HasMaxLength(7);
    }
}
```

- [ ] **Step 3: Criar DI da Infrastructure**

```csharp
// backend/src/Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Infrastructure.Persistence;

namespace SistemaTraction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
```

- [ ] **Step 4: Atualizar appsettings.json com LocalDB**

```json
// backend/src/API/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SistemaTraction;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 5: Atualizar Program.cs**

```csharp
// backend/src/API/Program.cs
using SistemaTraction.Application;
using SistemaTraction.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

- [ ] **Step 6: Gerar e aplicar migration**

```bash
cd backend
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API --output-dir Persistence/Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

Expected: banco criado em LocalDB com tabelas `FabricTypes` e `FabricColors`

- [ ] **Step 7: Build completo**

```bash
cd backend
dotnet build
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 8: Commit**

```bash
git add backend/src/Infrastructure/ backend/src/API/
git commit -m "feat: configurar EF Core, migrations InitialCreate e Program.cs"
```

---

## Task 9: API Controller — FabricTypesController

**Files:**
- Create: `backend/src/API/Controllers/FabricTypesController.cs`

- [ ] **Step 1: Criar controller**

```csharp
// backend/src/API/Controllers/FabricTypesController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SistemaTraction.Application.Fabric.Commands.CreateFabricColor;
using SistemaTraction.Application.Fabric.Commands.CreateFabricType;
using SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;
using SistemaTraction.Application.Fabric.Commands.DeleteFabricType;
using SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;
using SistemaTraction.Application.Fabric.Commands.UpdateFabricType;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;
using SistemaTraction.Application.Fabric.Queries.GetFabricTypes;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/fabric-types")]
public class FabricTypesController(IMediator mediator) : ControllerBase
{
    // GET api/fabric-types
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricTypesQuery(), ct);
        return Ok(result);
    }

    // GET api/fabric-types/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetFabricTypeByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/fabric-types
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFabricTypeRequest request, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(new CreateFabricTypeCommand(
                request.Name, request.Variation, request.PricePerKg,
                request.AverageKgPerRoll, request.AveragePiecesPerRoll), ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/fabric-types/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFabricTypeRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateFabricTypeCommand(
                id, request.Name, request.Variation, request.PricePerKg,
                request.AverageKgPerRoll, request.AveragePiecesPerRoll), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/fabric-types/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteFabricTypeCommand(id), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // POST api/fabric-types/{id}/colors
    [HttpPost("{id:guid}/colors")]
    public async Task<IActionResult> AddColor(Guid id, [FromBody] CreateFabricColorRequest request, CancellationToken ct)
    {
        try
        {
            var colorId = await mediator.Send(new CreateFabricColorCommand(id, request.Name, request.HexCode), ct);
            return Created(string.Empty, new { id = colorId });
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // PUT api/fabric-types/{id}/colors/{colorId}
    [HttpPut("{id:guid}/colors/{colorId:guid}")]
    public async Task<IActionResult> UpdateColor(Guid id, Guid colorId, [FromBody] UpdateFabricColorRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new UpdateFabricColorCommand(colorId, request.Name, request.HexCode), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // DELETE api/fabric-types/{id}/colors/{colorId}
    [HttpDelete("{id:guid}/colors/{colorId:guid}")]
    public async Task<IActionResult> DeleteColor(Guid id, Guid colorId, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteFabricColorCommand(colorId), ct);
            return NoContent();
        }
        catch (DomainException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

// Request records (input models — separados dos DTOs de resposta)
public record CreateFabricTypeRequest(string Name, string Variation, decimal PricePerKg, decimal AverageKgPerRoll, int? AveragePiecesPerRoll);
public record UpdateFabricTypeRequest(string Name, string Variation, decimal PricePerKg, decimal AverageKgPerRoll, int? AveragePiecesPerRoll);
public record CreateFabricColorRequest(string Name, string? HexCode);
public record UpdateFabricColorRequest(string Name, string? HexCode);
```

- [ ] **Step 2: Rodar API e validar Swagger**

```bash
cd backend
dotnet run --project src/API
```

Abrir http://localhost:5000/swagger e confirmar que os endpoints aparecem.

- [ ] **Step 3: Testar endpoints manualmente**

```bash
# Criar tipo de tecido
curl -X POST http://localhost:5000/api/fabric-types \
  -H "Content-Type: application/json" \
  -d '{"name":"Malha","variation":"Regular","pricePerKg":20.00,"averageKgPerRoll":10.0,"averagePiecesPerRoll":80}'

# Listar tipos
curl http://localhost:5000/api/fabric-types

# Adicionar cor
curl -X POST http://localhost:5000/api/fabric-types/{id}/colors \
  -H "Content-Type: application/json" \
  -d '{"name":"Preto","hexCode":"#000000"}'
```

Expected: 201 Created com id nos dois POSTs, 200 com lista na query

- [ ] **Step 4: Commit**

```bash
git add backend/src/API/Controllers/
git commit -m "feat: adicionar FabricTypesController com CRUD completo + cores"
```

---

## Task 10: Setup do frontend React

**Files:**
- Create: `frontend/` (via pnpm create vite)
- Create: `frontend/src/lib/api.ts`
- Create: `frontend/src/lib/queryClient.ts`

- [ ] **Step 1: Criar projeto React com Vite**

```bash
cd "C:\Users\andre\OneDrive\Desktop\Sistema Traction"
pnpm create vite frontend --template react-ts
```

- [ ] **Step 2: Instalar dependências**

```bash
cd frontend
pnpm install
pnpm add @tanstack/react-query @tanstack/react-query-devtools
pnpm add react-hook-form @hookform/resolvers zod
pnpm add zustand
pnpm add axios
pnpm add -D tailwindcss @tailwindcss/vite
pnpm add -D vitest @vitest/ui @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom
```

- [ ] **Step 3: Configurar Tailwind**

```ts
// frontend/vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5000',
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test-setup.ts',
  },
})
```

```css
/* frontend/src/index.css */
@import "tailwindcss";
```

- [ ] **Step 4: Instalar e configurar shadcn/ui**

```bash
cd frontend
pnpm dlx shadcn@latest init
```

Responder ao wizard:
- Style: Default
- Base color: Neutral
- CSS variables: Yes

Depois instalar componentes base:

```bash
pnpm dlx shadcn@latest add button input label form dialog table badge toast
```

- [ ] **Step 5: Criar API client**

```ts
// frontend/src/lib/api.ts
import axios from 'axios'

export const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.error ?? 'Erro inesperado.'
    return Promise.reject(new Error(message))
  }
)
```

- [ ] **Step 6: Criar QueryClient**

```ts
// frontend/src/lib/queryClient.ts
import { QueryClient } from '@tanstack/react-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60,      // 1 minuto
      retry: 1,
    },
  },
})
```

- [ ] **Step 7: Atualizar main.tsx**

```tsx
// frontend/src/main.tsx
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { queryClient } from '@/lib/queryClient'
import App from './App.tsx'
import './index.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </StrictMode>,
)
```

- [ ] **Step 8: Criar test-setup**

```ts
// frontend/src/test-setup.ts
import '@testing-library/jest-dom'
```

- [ ] **Step 9: Verificar que dev server sobe**

```bash
cd frontend
pnpm dev
```

Expected: Vite rodando em http://localhost:5173

- [ ] **Step 10: Commit**

```bash
cd "C:\Users\andre\OneDrive\Desktop\Sistema Traction"
git add frontend/
git commit -m "feat: setup frontend React 18 + TypeScript + Vite + Tailwind + shadcn/ui"
```

---

## Task 11: Feature de FabricType no frontend

**Files:**
- Create: `frontend/src/features/settings/fabric/schemas/fabricTypeSchema.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useFabricTypes.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useCreateFabricType.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useUpdateFabricType.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useDeleteFabricType.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useCreateFabricColor.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useUpdateFabricColor.ts`
- Create: `frontend/src/features/settings/fabric/hooks/useDeleteFabricColor.ts`
- Create: `frontend/src/features/settings/fabric/components/FabricTypeList.tsx`
- Create: `frontend/src/features/settings/fabric/components/FabricTypeForm.tsx`
- Create: `frontend/src/features/settings/fabric/components/FabricColorForm.tsx`
- Create: `frontend/src/features/settings/fabric/FabricTypePage.tsx`

- [ ] **Step 1: Criar schemas Zod**

```ts
// frontend/src/features/settings/fabric/schemas/fabricTypeSchema.ts
import { z } from 'zod'

export const fabricTypeSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100),
  variation: z.string().min(1, 'Variação é obrigatória').max(100),
  pricePerKg: z.coerce.number().positive('Preço deve ser maior que zero'),
  averageKgPerRoll: z.coerce.number().positive('Média de kg deve ser maior que zero'),
  averagePiecesPerRoll: z.coerce.number().int().positive().optional().nullable(),
})

export const fabricColorSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100),
  hexCode: z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Formato inválido').optional().or(z.literal('')).nullable(),
})

export type FabricTypeFormData = z.infer<typeof fabricTypeSchema>
export type FabricColorFormData = z.infer<typeof fabricColorSchema>
```

- [ ] **Step 2: Criar tipos TypeScript**

```ts
// frontend/src/features/settings/fabric/types.ts
export interface FabricColorDto {
  id: string
  fabricTypeId: string
  name: string
  hexCode: string | null
}

export interface FabricTypeDto {
  id: string
  name: string
  variation: string
  pricePerKg: number
  averageKgPerRoll: number
  averagePiecesPerRoll: number | null
  colors: FabricColorDto[]
}
```

- [ ] **Step 3: Criar hooks TanStack Query**

```ts
// frontend/src/features/settings/fabric/hooks/useFabricTypes.ts
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeDto } from '../types'

export function useFabricTypes() {
  return useQuery({
    queryKey: ['fabric-types'],
    queryFn: async () => {
      const { data } = await api.get<FabricTypeDto[]>('/fabric-types')
      return data
    },
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useCreateFabricType.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeFormData } from '../schemas/fabricTypeSchema'

export function useCreateFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: FabricTypeFormData) =>
      api.post('/fabric-types', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useUpdateFabricType.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricTypeFormData } from '../schemas/fabricTypeSchema'

export function useUpdateFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: FabricTypeFormData }) =>
      api.put(`/fabric-types/${id}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useDeleteFabricType.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteFabricType() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/fabric-types/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useCreateFabricColor.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricColorFormData } from '../schemas/fabricTypeSchema'

export function useCreateFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: FabricColorFormData) =>
      api.post(`/fabric-types/${fabricTypeId}/colors`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useUpdateFabricColor.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { FabricColorFormData } from '../schemas/fabricTypeSchema'

export function useUpdateFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ colorId, data }: { colorId: string; data: FabricColorFormData }) =>
      api.put(`/fabric-types/${fabricTypeId}/colors/${colorId}`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

```ts
// frontend/src/features/settings/fabric/hooks/useDeleteFabricColor.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDeleteFabricColor(fabricTypeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (colorId: string) =>
      api.delete(`/fabric-types/${fabricTypeId}/colors/${colorId}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fabric-types'] }),
  })
}
```

- [ ] **Step 4: Criar FabricTypeForm**

```tsx
// frontend/src/features/settings/fabric/components/FabricTypeForm.tsx
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { fabricTypeSchema, type FabricTypeFormData } from '../schemas/fabricTypeSchema'
import type { FabricTypeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<FabricTypeDto>
  onSubmit: (data: FabricTypeFormData) => void
  isLoading?: boolean
}

export function FabricTypeForm({ defaultValues, onSubmit, isLoading }: Props) {
  const { register, handleSubmit, formState: { errors } } = useForm<FabricTypeFormData>({
    resolver: zodResolver(fabricTypeSchema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      variation: defaultValues?.variation ?? '',
      pricePerKg: defaultValues?.pricePerKg ?? undefined,
      averageKgPerRoll: defaultValues?.averageKgPerRoll ?? undefined,
      averagePiecesPerRoll: defaultValues?.averagePiecesPerRoll ?? undefined,
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Label htmlFor="name">Nome</Label>
        <Input id="name" {...register('name')} placeholder="ex: Malha" />
        {errors.name && <p className="text-sm text-red-500">{errors.name.message}</p>}
      </div>
      <div>
        <Label htmlFor="variation">Variação</Label>
        <Input id="variation" {...register('variation')} placeholder="ex: Regular" />
        {errors.variation && <p className="text-sm text-red-500">{errors.variation.message}</p>}
      </div>
      <div>
        <Label htmlFor="pricePerKg">Preço por kg (R$)</Label>
        <Input id="pricePerKg" type="number" step="0.01" {...register('pricePerKg')} />
        {errors.pricePerKg && <p className="text-sm text-red-500">{errors.pricePerKg.message}</p>}
      </div>
      <div>
        <Label htmlFor="averageKgPerRoll">Média de kg por bobina</Label>
        <Input id="averageKgPerRoll" type="number" step="0.1" {...register('averageKgPerRoll')} />
        {errors.averageKgPerRoll && <p className="text-sm text-red-500">{errors.averageKgPerRoll.message}</p>}
      </div>
      <div>
        <Label htmlFor="averagePiecesPerRoll">Peças por bobina (opcional)</Label>
        <Input id="averagePiecesPerRoll" type="number" {...register('averagePiecesPerRoll')} />
      </div>
      <Button type="submit" disabled={isLoading}>
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
```

- [ ] **Step 5: Criar FabricColorForm**

```tsx
// frontend/src/features/settings/fabric/components/FabricColorForm.tsx
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { fabricColorSchema, type FabricColorFormData } from '../schemas/fabricTypeSchema'
import type { FabricColorDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  defaultValues?: Partial<FabricColorDto>
  onSubmit: (data: FabricColorFormData) => void
  isLoading?: boolean
}

export function FabricColorForm({ defaultValues, onSubmit, isLoading }: Props) {
  const { register, handleSubmit, formState: { errors } } = useForm<FabricColorFormData>({
    resolver: zodResolver(fabricColorSchema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      hexCode: defaultValues?.hexCode ?? '',
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Label htmlFor="colorName">Nome da cor</Label>
        <Input id="colorName" {...register('name')} placeholder="ex: Preto" />
        {errors.name && <p className="text-sm text-red-500">{errors.name.message}</p>}
      </div>
      <div>
        <Label htmlFor="hexCode">Cor hex (opcional)</Label>
        <div className="flex gap-2 items-center">
          <Input id="hexCode" {...register('hexCode')} placeholder="#000000" className="font-mono" />
        </div>
        {errors.hexCode && <p className="text-sm text-red-500">{errors.hexCode.message}</p>}
      </div>
      <Button type="submit" disabled={isLoading}>
        {isLoading ? 'Salvando...' : 'Salvar'}
      </Button>
    </form>
  )
}
```

- [ ] **Step 6: Criar FabricTypeList**

```tsx
// frontend/src/features/settings/fabric/components/FabricTypeList.tsx
import { useState } from 'react'
import { useFabricTypes } from '../hooks/useFabricTypes'
import { useDeleteFabricType } from '../hooks/useDeleteFabricType'
import { useCreateFabricColor } from '../hooks/useCreateFabricColor'
import { useDeleteFabricColor } from '../hooks/useDeleteFabricColor'
import { FabricColorForm } from './FabricColorForm'
import type { FabricTypeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'

interface Props {
  onEdit: (type: FabricTypeDto) => void
}

export function FabricTypeList({ onEdit }: Props) {
  const { data: types, isLoading, error } = useFabricTypes()
  const deleteType = useDeleteFabricType()
  const [addColorTo, setAddColorTo] = useState<string | null>(null)
  const createColor = useCreateFabricColor(addColorTo ?? '')
  const deleteColor = useDeleteFabricColor(addColorTo ?? '')

  if (isLoading) return <p className="text-muted-foreground">Carregando...</p>
  if (error) return <p className="text-red-500">Erro ao carregar tipos de tecido.</p>
  if (!types?.length) return <p className="text-muted-foreground">Nenhum tipo cadastrado.</p>

  return (
    <div className="space-y-4">
      {types.map((type) => (
        <div key={type.id} className="border rounded-lg p-4 space-y-3">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-semibold">{type.name} — {type.variation}</h3>
              <p className="text-sm text-muted-foreground">
                R$ {type.pricePerKg.toFixed(2)}/kg · {type.averageKgPerRoll}kg/bobina
                {type.averagePiecesPerRoll ? ` · ~${type.averagePiecesPerRoll} peças` : ''}
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={() => onEdit(type)}>Editar</Button>
              <Button
                variant="destructive"
                size="sm"
                onClick={() => deleteType.mutate(type.id)}
                disabled={deleteType.isPending}
              >
                Excluir
              </Button>
            </div>
          </div>

          <div className="flex flex-wrap gap-2 items-center">
            {type.colors.map((color) => (
              <div key={color.id} className="flex items-center gap-1">
                {color.hexCode && (
                  <span
                    className="inline-block w-3 h-3 rounded-full border"
                    style={{ backgroundColor: color.hexCode }}
                  />
                )}
                <Badge variant="secondary">{color.name}</Badge>
                <button
                  className="text-xs text-red-500 hover:underline"
                  onClick={() => {
                    setAddColorTo(type.id)
                    deleteColor.mutate(color.id)
                  }}
                >
                  ×
                </button>
              </div>
            ))}

            <Dialog>
              <DialogTrigger asChild>
                <Button variant="ghost" size="sm" onClick={() => setAddColorTo(type.id)}>
                  + Cor
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Adicionar cor — {type.name} {type.variation}</DialogTitle>
                </DialogHeader>
                <FabricColorForm
                  onSubmit={(data) => createColor.mutate(data)}
                  isLoading={createColor.isPending}
                />
              </DialogContent>
            </Dialog>
          </div>
        </div>
      ))}
    </div>
  )
}
```

- [ ] **Step 7: Criar FabricTypePage**

```tsx
// frontend/src/features/settings/fabric/FabricTypePage.tsx
import { useState } from 'react'
import { FabricTypeList } from './components/FabricTypeList'
import { FabricTypeForm } from './components/FabricTypeForm'
import { useCreateFabricType } from './hooks/useCreateFabricType'
import { useUpdateFabricType } from './hooks/useUpdateFabricType'
import type { FabricTypeDto } from './types'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

export function FabricTypePage() {
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<FabricTypeDto | null>(null)
  const createType = useCreateFabricType()
  const updateType = useUpdateFabricType()

  function handleEdit(type: FabricTypeDto) {
    setEditing(type)
    setOpen(true)
  }

  function handleClose() {
    setOpen(false)
    setEditing(null)
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">Tipos de Tecido</h2>
          <p className="text-muted-foreground">Gerencie os tipos, variações e cores dos tecidos.</p>
        </div>
        <Button onClick={() => setOpen(true)}>+ Novo tipo</Button>
      </div>

      <FabricTypeList onEdit={handleEdit} />

      <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose() }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar tipo de tecido' : 'Novo tipo de tecido'}</DialogTitle>
          </DialogHeader>
          <FabricTypeForm
            defaultValues={editing ?? undefined}
            isLoading={createType.isPending || updateType.isPending}
            onSubmit={(data) => {
              if (editing) {
                updateType.mutate(
                  { id: editing.id, data },
                  { onSuccess: handleClose }
                )
              } else {
                createType.mutate(data, { onSuccess: handleClose })
              }
            }}
          />
        </DialogContent>
      </Dialog>
    </div>
  )
}
```

- [ ] **Step 8: Conectar no App.tsx**

```tsx
// frontend/src/App.tsx
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'

export default function App() {
  return (
    <main className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-8">SistemaTraction</h1>
      <FabricTypePage />
    </main>
  )
}
```

- [ ] **Step 9: Rodar typecheck e lint**

```bash
cd frontend
pnpm typecheck && pnpm lint
```

Expected: sem erros

- [ ] **Step 10: Verificar integração com backend rodando**

```bash
# Terminal 1 — backend
cd backend && dotnet run --project src/API

# Terminal 2 — frontend
cd frontend && pnpm dev
```

Abrir http://localhost:5173, criar um tipo de tecido, adicionar cores, editar e excluir.

- [ ] **Step 11: Commit**

```bash
git add frontend/src/
git commit -m "feat: adicionar feature FabricType no frontend com CRUD completo e cores"
```

---

## Task 12: Rodar todos os testes e verificação final

- [ ] **Step 1: Testes do backend**

```bash
cd backend
dotnet test --verbosity normal
```

Expected: todos PASS

- [ ] **Step 2: Typecheck do frontend**

```bash
cd frontend
pnpm typecheck
```

Expected: 0 erros

- [ ] **Step 3: Build de produção do frontend**

```bash
cd frontend
pnpm build
```

Expected: `dist/` gerado sem erros

- [ ] **Step 4: Commit final**

```bash
git add .
git commit -m "chore: verificação final — setup + Módulo 1A concluídos"
```

---

## Endpoints criados (Módulo 1A)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/fabric-types` | Listar todos os tipos (com cores) |
| GET | `/api/fabric-types/{id}` | Buscar tipo por ID |
| POST | `/api/fabric-types` | Criar novo tipo |
| PUT | `/api/fabric-types/{id}` | Atualizar tipo |
| DELETE | `/api/fabric-types/{id}` | Soft-delete tipo |
| POST | `/api/fabric-types/{id}/colors` | Adicionar cor ao tipo |
| PUT | `/api/fabric-types/{id}/colors/{colorId}` | Atualizar cor |
| DELETE | `/api/fabric-types/{id}/colors/{colorId}` | Soft-delete cor |

---

## O que vem no próximo plano (Módulo 1B)

- Entidade `DtfModel` com CRUD
- Seed automático dos 6 modelos cadastrados na spec
- `DtfStockItem` + `DtfStockMovement` (append-only)
- AppConfig (key-value seed com valores padrão)
