# SKU Formato Modelagem-Estampa-Cor-Tamanho — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fazer o upload de lista de separação reconhecer SKUs de 4 segmentos (`MODELAGEM-ESTAMPA-COR-TAMANHO`, ex: `REG-MADT-RED-G`) do novo PDF "Lista de Resumo" do UpSeller, adicionando o campo Estampa (vinculado a um `DtfModel` cadastrado) sem alterar a checagem/dedução de estoque de camisetas.

**Architecture:** Estende o domínio existente (`SkuCode`, `SeparationItem`) com um novo campo/categoria Estampa; o parser de SKU em `UploadSeparationListCommandHandler` passa a detectar 3 ou 4 segmentos e resolver Estampa via lookup de `SkuCode` (categoria Estampa → `DtfModelId`). DTOs e UI (tabela de revisão + painel de config. SKU) ganham a nova coluna/campo.

**Tech Stack:** .NET 8 (EF Core, MediatR, xUnit/FluentAssertions), React + TypeScript (TanStack Query), SQL Server.

---

## Spec de referência

`docs/superpowers/specs/2026-06-26-sku-formato-modelagem-estampa-cor-tamanho-design.md`

## Mapa de arquivos

**Backend — domínio:**
- Modify: `backend/src/Domain/Separation/SkuCodeCategory.cs` — adiciona `Estampa`.
- Modify: `backend/src/Domain/Separation/SkuCode.cs` — adiciona `DtfModelId`.
- Modify: `backend/src/Domain/Separation/SeparationItem.cs` — adiciona `Estampa`.

**Backend — persistência:**
- Modify: `backend/src/Infrastructure/Persistence/Configurations/SkuCodeConfiguration.cs`
- Modify: `backend/src/Infrastructure/Persistence/Configurations/SeparationItemConfiguration.cs`
- Create: migration `AddEstampaToSkuCodeAndSeparationItem`
- Modify: `backend/tests/Application.Tests/TestApplicationDbContext.cs`

**Backend — aplicação:**
- Modify: `backend/src/Application/Separation/DTOs/SkuCodeDto.cs`
- Modify: `backend/src/Application/Separation/DTOs/SeparationListDto.cs`
- Modify: `backend/src/Application/Separation/Commands/UpsertSkuCode/UpsertSkuCodeCommand.cs`
- Modify: `backend/src/Application/Separation/Commands/UpsertSkuCode/UpsertSkuCodeCommandHandler.cs`
- Modify: `backend/src/Application/Separation/Commands/UploadSeparationList/UploadSeparationListCommandHandler.cs`
- Modify: `backend/src/Application/Separation/Queries/GetSkuCodes/GetSkuCodesQueryHandler.cs` (se precisar mapear novo campo)

**Backend — testes:**
- Create: `backend/tests/Application.Tests/Separation/UpsertSkuCodeCommandHandlerTests.cs`
- Create: `backend/tests/Application.Tests/Separation/UploadSeparationListCommandHandlerTests.cs`

**Frontend:**
- Modify: `frontend/src/features/separation/types.ts`
- Modify: `frontend/src/features/separation/SeparationListPage.tsx`
- Modify: `frontend/src/features/separation/components/SkuConfigPanel.tsx`

---

## Task 1: Domínio — `SkuCodeCategory` ganha `Estampa`

**Files:**
- Modify: `backend/src/Domain/Separation/SkuCodeCategory.cs`

- [ ] **Step 1: Editar o enum**

```csharp
namespace SistemaTraction.Domain.Separation;

public enum SkuCodeCategory
{
    Modelo,
    Estampa,
    Cor,
    Tamanho
}
```

- [ ] **Step 2: Compilar para garantir que nada quebrou**

Run: `cd backend && dotnet build`
Expected: Build succeeded (0 erros).

- [ ] **Step 3: Commit**

```bash
cd backend
git add src/Domain/Separation/SkuCodeCategory.cs
git commit -m "feat: adiciona categoria Estampa ao SkuCodeCategory"
```

---

## Task 2: Domínio — `SkuCode.DtfModelId`

**Files:**
- Modify: `backend/src/Domain/Separation/SkuCode.cs`

- [ ] **Step 1: Adicionar campo e validação**

Substituir o conteúdo de `SkuCode.cs` por:

```csharp
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Separation;

public class SkuCode : BaseEntity
{
    public string Code { get; private set; } = "";
    public string Value { get; private set; } = "";
    public SkuCodeCategory Category { get; private set; }
    public Guid? DtfModelId { get; private set; }

    private SkuCode() { }

    public static SkuCode Create(string code, string value, SkuCodeCategory category, Guid? dtfModelId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código SKU não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Valor do código SKU não pode ser vazio.");
        if (dtfModelId.HasValue && category != SkuCodeCategory.Estampa)
            throw new DomainException("DtfModelId só pode ser informado para a categoria Estampa.");
        if (category == SkuCodeCategory.Estampa && !dtfModelId.HasValue)
            throw new DomainException("Selecione um modelo DTF para a categoria Estampa.");

        return new SkuCode
        {
            Code = code.Trim().ToUpper(),
            Value = value.Trim(),
            Category = category,
            DtfModelId = dtfModelId,
        };
    }

    public void Update(string code, string value, SkuCodeCategory category, Guid? dtfModelId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código SKU não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Valor do código SKU não pode ser vazio.");
        if (dtfModelId.HasValue && category != SkuCodeCategory.Estampa)
            throw new DomainException("DtfModelId só pode ser informado para a categoria Estampa.");
        if (category == SkuCodeCategory.Estampa && !dtfModelId.HasValue)
            throw new DomainException("Selecione um modelo DTF para a categoria Estampa.");

        Code = code.Trim().ToUpper();
        Value = value.Trim();
        Category = category;
        DtfModelId = dtfModelId;
        TouchUpdatedAt();
    }
}
```

- [ ] **Step 2: Compilar**

Run: `cd backend && dotnet build`
Expected: Build falha em `UpsertSkuCodeCommandHandler.cs` (chamadas a `Create`/`Update` ainda compilam pois `dtfModelId` é opcional — então deve compilar limpo). Confirmar: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
cd backend
git add src/Domain/Separation/SkuCode.cs
git commit -m "feat: adiciona DtfModelId ao SkuCode para categoria Estampa"
```

---

## Task 3: Domínio — `SeparationItem.Estampa`

**Files:**
- Modify: `backend/src/Domain/Separation/SeparationItem.cs`

- [ ] **Step 1: Adicionar campo `Estampa`**

Substituir o conteúdo de `SeparationItem.cs` por:

```csharp
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Domain.Separation;

public class SeparationItem : BaseEntity
{
    public Guid SeparationListId { get; private set; }
    public string Sku { get; private set; } = "";
    public string Estampa { get; private set; } = "";
    public string Color { get; private set; } = "";
    public string Size { get; private set; } = "";
    public int Quantity { get; private set; }
    public Guid? DtfModelId { get; private set; }
    public int SortOrder { get; private set; }

    public SeparationList? SeparationList { get; private set; }
    public DtfModel? DtfModel { get; private set; }

    private SeparationItem() { }

    public static SeparationItem Create(
        Guid separationListId, string sku, string estampa, string color, string size, int quantity, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Cor é obrigatória.");

        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        return new SeparationItem
        {
            SeparationListId = separationListId,
            Sku = sku?.Trim() ?? "",
            Estampa = estampa?.Trim() ?? "",
            Color = color.Trim(),
            Size = size.Trim().ToUpper(),
            Quantity = quantity,
            SortOrder = sortOrder
        };
    }

    public void Update(string sku, string estampa, string color, string size, int quantity)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Cor é obrigatória.");

        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        Sku = sku?.Trim() ?? "";
        Estampa = estampa?.Trim() ?? "";
        Color = color.Trim();
        Size = size.Trim().ToUpper();
        Quantity = quantity;
        TouchUpdatedAt();
    }

    public void SetDtfModel(Guid? dtfModelId)
    {
        DtfModelId = dtfModelId;
        TouchUpdatedAt();
    }
}
```

> **Nota:** isso muda a assinatura de `Create`/`Update` (novo parâmetro `estampa`). Os chamadores serão atualizados na Task 7 (`UploadSeparationListCommandHandler`) e na Task 8 (`UpdateSeparationItemsCommandHandler`). O build vai falhar até essas tasks — isso é esperado neste ponto do plano.

- [ ] **Step 2: Commit (build vai falhar — comitar mesmo assim, próximas tasks corrigem)**

```bash
cd backend
git add src/Domain/Separation/SeparationItem.cs
git commit -m "feat: adiciona campo Estampa ao SeparationItem"
```

---

## Task 4: Persistência — configurações EF Core

**Files:**
- Modify: `backend/src/Infrastructure/Persistence/Configurations/SkuCodeConfiguration.cs`
- Modify: `backend/src/Infrastructure/Persistence/Configurations/SeparationItemConfiguration.cs`

- [ ] **Step 1: Atualizar `SkuCodeConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SkuCodeConfiguration : IEntityTypeConfiguration<SkuCode>
{
    public void Configure(EntityTypeBuilder<SkuCode> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Value).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<DtfModel>()
            .WithMany()
            .HasForeignKey(c => c.DtfModelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

- [ ] **Step 2: Atualizar `SeparationItemConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SeparationItemConfiguration : IEntityTypeConfiguration<SeparationItem>
{
    public void Configure(EntityTypeBuilder<SeparationItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).HasMaxLength(100);
        builder.Property(i => i.Estampa).HasMaxLength(100);
        builder.Property(i => i.Color).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Size).HasMaxLength(10).IsRequired();

        // Relationship to SeparationList is configured from the SeparationList side

        builder.HasOne(i => i.DtfModel)
            .WithMany()
            .HasForeignKey(i => i.DtfModelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.SeparationListId);
    }
}
```

- [ ] **Step 3: Commit**

```bash
cd backend
git add src/Infrastructure/Persistence/Configurations/SkuCodeConfiguration.cs src/Infrastructure/Persistence/Configurations/SeparationItemConfiguration.cs
git commit -m "feat: configura DtfModelId em SkuCode e coluna Estampa em SeparationItem"
```

---

## Task 5: Migration EF Core

**Files:**
- Create: migration `AddEstampaToSkuCodeAndSeparationItem` em `backend/src/Infrastructure/Persistence/Migrations/`

> Pré-requisito: Tasks 1–4 concluídas (build do projeto `Infrastructure` deve compilar; o projeto todo só compila após a Task 7/8, mas a ferramenta `dotnet ef migrations add` só precisa do projeto `Infrastructure`+`Domain` consistentes — se o build falhar por causa de `UploadSeparationListCommandHandler`/`UpdateSeparationItemsCommandHandler` ainda não corrigidos, adiantar essas correções de assinatura agora não é necessário porque `dotnet ef` compila a solução inteira. Por isso, execute esta task **depois** da Task 8.)

- [ ] **Step 1: Gerar a migration**

Run (depois que Tasks 1–8 estiverem com o build verde):
```bash
cd backend
dotnet ef migrations add AddEstampaToSkuCodeAndSeparationItem --project src/Infrastructure --startup-project src/API
```
Expected: cria `src/Infrastructure/Persistence/Migrations/<timestamp>_AddEstampaToSkuCodeAndSeparationItem.cs` e o `.Designer.cs`, e atualiza `ApplicationDbContextModelSnapshot.cs` com:
- coluna `DtfModelId` (nullable, `uniqueidentifier`) em `SkuCodes`, com FK para `DtfModels`.
- coluna `Estampa` (nullable `nvarchar(100)`) em `SeparationItems`.

- [ ] **Step 2: Revisar a migration gerada**

Abrir o arquivo `<timestamp>_AddEstampaToSkuCodeAndSeparationItem.cs` e confirmar que `Up()` contém `AddColumn` para `SkuCodes.DtfModelId` e `SeparationItems.Estampa`, e `AddForeignKey` para `SkuCodes.DtfModelId → DtfModels.Id` com `onDelete: ReferentialAction.SetNull`. Se algo estiver diferente (ex: tipo de coluna incorreto), corrigir manualmente antes de aplicar.

- [ ] **Step 3: Aplicar a migration no banco local**

Run: `dotnet ef database update --project src/Infrastructure --startup-project src/API`
Expected: `Done.`

- [ ] **Step 4: Commit**

```bash
cd backend
git add src/Infrastructure/Persistence/Migrations/
git commit -m "feat: migration AddEstampaToSkuCodeAndSeparationItem"
```

---

## Task 6: Atualizar `TestApplicationDbContext` (InMemory) para os novos campos

**Files:**
- Modify: `backend/tests/Application.Tests/TestApplicationDbContext.cs`

- [ ] **Step 1: Editar o bloco `SkuCode` em `OnModelCreating`**

Trocar:
```csharp
        modelBuilder.Entity<SkuCode>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Category).HasConversion<string>();
        });
```
Por:
```csharp
        modelBuilder.Entity<SkuCode>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Category).HasConversion<string>();
            b.HasOne<DtfModel>().WithMany().HasForeignKey(c => c.DtfModelId).IsRequired(false);
        });
```

(`DtfModel` já está importado via `using SistemaTraction.Domain.Dtf;` no topo do arquivo.)

- [ ] **Step 2: Compilar o projeto de testes**

Run: `cd backend && dotnet build tests/Application.Tests`
Expected: Build succeeded (assumindo Tasks 7–8 já feitas; se ainda não, vai falhar nas chamadas de `SeparationItem.Create`/`Update` — normal neste ponto).

- [ ] **Step 3: Commit**

```bash
cd backend
git add tests/Application.Tests/TestApplicationDbContext.cs
git commit -m "test: configura DtfModelId do SkuCode no contexto de testes em memória"
```

---

## Task 7: Parser de SKU — `UploadSeparationListCommandHandler`

**Files:**
- Modify: `backend/src/Application/Separation/Commands/UploadSeparationList/UploadSeparationListCommandHandler.cs`

- [ ] **Step 1: Substituir o handler inteiro**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.UploadSeparationList;

public class UploadSeparationListCommandHandler(IApplicationDbContext context, IPdfParser pdfParser)
    : IRequestHandler<UploadSeparationListCommand, SeparationListDetailDto>
{
    public async Task<SeparationListDetailDto> Handle(
        UploadSeparationListCommand request, CancellationToken cancellationToken)
    {
        var parsed = pdfParser.Parse(request.PdfStream, request.FileName);
        if (!parsed.Success)
            throw new DomainException(
                $"Não foi possível processar o PDF: {parsed.ErrorMessage ?? "formato não reconhecido"}. " +
                "Verifique se o arquivo é uma lista de separação válida do ERP e se os Códigos SKU estão configurados.");

        // Load SKU code mappings (Modelo, Estampa, Cor, Tamanho)
        var skuCodes = await context.SkuCodes
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var skuLookup = skuCodes.ToLookup(c => c.Code, StringComparer.OrdinalIgnoreCase);

        var list = SeparationList.Create(request.FileName);
        context.SeparationLists.Add(list);
        await context.SaveChangesAsync(cancellationToken);

        var items = new List<SeparationItem>();
        foreach (var p in parsed.Items)
        {
            var resolved = ResolveSkuParts(p.Sku, skuLookup);

            var resolvedColor = !string.IsNullOrWhiteSpace(p.Color) ? p.Color : (resolved.Color ?? "");
            var resolvedSize  = !string.IsNullOrWhiteSpace(p.Size)  ? p.Size  : (resolved.Size  ?? "");

            if (string.IsNullOrWhiteSpace(resolvedColor)) resolvedColor = "?";
            if (string.IsNullOrWhiteSpace(resolvedSize))  resolvedSize  = "?";

            var item = SeparationItem.Create(
                list.Id, p.Sku, resolved.Estampa ?? "", resolvedColor, resolvedSize, p.Quantity, p.SortOrder);

            if (resolved.DtfModelId.HasValue)
                item.SetDtfModel(resolved.DtfModelId);

            items.Add(item);
        }

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Estampa, i.Color, i.Size, i.Quantity, i.SortOrder, i.DtfModelId)).ToList()
        );
    }

    /// <summary>
    /// Parses a SKU. Dois formatos suportados:
    /// - 4+ segmentos: MODELAGEM-ESTAMPA-COR-TAMANHO (ex: REG-MADT-RED-G).
    ///   Posição 1 → Estampa (lookup Category=Estampa, retorna também DtfModelId).
    ///   Posição 2 → Cor (lookup Category=Cor).
    ///   Posição 3 → Tamanho (lookup Category=Tamanho).
    /// - Exatamente 3 segmentos (formato legado): MODELO-COR-TAMANHO (ex: BBL-BLK-M).
    ///   Posição 1 → Cor. Posição 2 → Tamanho. Sem Estampa.
    ///
    /// Posição 0 (Modelagem/Modelo) não é resolvida aqui — usada apenas como ModelCode
    /// bruto em outras partes do fluxo (checagem/dedução de estoque de camiseta).
    /// Segmentos extras (5º em diante) são ignorados.
    /// Se um segmento não tiver mapeamento configurado, o código bruto é usado como fallback.
    /// </summary>
    private static (string? Color, string? Size, string? Estampa, Guid? DtfModelId) ResolveSkuParts(
        string sku,
        ILookup<string, SkuCode> skuLookup)
    {
        if (string.IsNullOrWhiteSpace(sku)) return (null, null, null, null);

        var parts = sku.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 4)
        {
            var estampaPart = parts[1].ToUpper();
            var estampaMatch = skuLookup[estampaPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Estampa);
            var estampa = estampaMatch?.Value ?? estampaPart;
            var dtfModelId = estampaMatch?.DtfModelId;

            var corPart = parts[2].ToUpper();
            var corMatch = skuLookup[corPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Cor);
            var cor = corMatch?.Value ?? corPart;

            var tamanhoPart = parts[3].ToUpper();
            var tamanhoMatch = skuLookup[tamanhoPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Tamanho);
            var tamanho = tamanhoMatch?.Value ?? tamanhoPart;

            return (cor, tamanho, estampa, dtfModelId);
        }

        if (parts.Length == 3)
        {
            var corPart = parts[1].ToUpper();
            var corMatch = skuLookup[corPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Cor);
            var cor = corMatch?.Value ?? corPart;

            var tamanhoPart = parts[2].ToUpper();
            var tamanhoMatch = skuLookup[tamanhoPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Tamanho);
            var tamanho = tamanhoMatch?.Value ?? tamanhoPart;

            return (cor, tamanho, null, null);
        }

        return (null, null, null, null);
    }
}
```

- [ ] **Step 2: Compilar**

Run: `cd backend && dotnet build`
Expected: ainda falha em `UpdateSeparationItemsCommandHandler.cs` (chama `SeparationItem.Update` com a assinatura antiga) e em `SeparationListDto.cs`/`SeparationItemDto` (assinatura mudou) — resolvido nas próximas tasks. Anotar os erros exatos reportados pelo compilador para confirmar que são só esses dois pontos.

- [ ] **Step 3: Commit**

```bash
cd backend
git add src/Application/Separation/Commands/UploadSeparationList/UploadSeparationListCommandHandler.cs
git commit -m "feat: parser de SKU reconhece formato Modelagem-Estampa-Cor-Tamanho"
```

---

## Task 8: DTOs — `SeparationItemDto` e `SkuCodeDto`

**Files:**
- Modify: `backend/src/Application/Separation/DTOs/SeparationListDto.cs`
- Modify: `backend/src/Application/Separation/DTOs/SkuCodeDto.cs`
- Modify: `backend/src/Application/Separation/Commands/UpdateSeparationItems/UpdateSeparationItemsCommand.cs`
- Modify: `backend/src/Application/Separation/Commands/UpdateSeparationItems/UpdateSeparationItemsCommandHandler.cs`
- Modify: `backend/src/Application/Separation/Queries/GetSkuCodes/GetSkuCodesQueryHandler.cs`
- Modify: `backend/src/Application/Separation/Queries/GetSeparationListById/GetSeparationListByIdQueryHandler.cs`

- [ ] **Step 1: Atualizar `SeparationItemDto` em `SeparationListDto.cs`**

Trocar:
```csharp
public record SeparationItemDto(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    int Quantity,
    int SortOrder
);
```
Por:
```csharp
public record SeparationItemDto(
    Guid Id,
    string Sku,
    string Estampa,
    string Color,
    string Size,
    int Quantity,
    int SortOrder,
    Guid? DtfModelId
);
```

- [ ] **Step 2: Atualizar `SkuCodeDto.cs`**

```csharp
namespace SistemaTraction.Application.Separation.DTOs;

public record SkuCodeDto(Guid Id, string Code, string Value, string Category, Guid? DtfModelId);
```

- [ ] **Step 3: Atualizar `UpdateSeparationItemsCommand.cs`**

Trocar:
```csharp
public record UpdateItemDto(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    int Quantity
);
```
Por:
```csharp
public record UpdateItemDto(
    Guid Id,
    string Sku,
    string Estampa,
    string Color,
    string Size,
    int Quantity
);
```

- [ ] **Step 4: Atualizar `UpdateSeparationItemsCommandHandler.cs`**

Trocar:
```csharp
        foreach (var dto in request.Items)
        {
            var item = items.FirstOrDefault(i => i.Id == dto.Id)
                ?? throw new DomainException($"Item {dto.Id} não encontrado.");

            item.Update(dto.Sku, dto.Color, dto.Size, dto.Quantity);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id,
            list.FileName,
            list.UploadedAt,
            list.Status.ToString(),
            items.OrderBy(i => i.SortOrder)
                 .Select(i => new SeparationItemDto(
                     i.Id, i.Sku, i.Color, i.Size, i.Quantity, i.SortOrder))
                 .ToList()
        );
```
Por:
```csharp
        foreach (var dto in request.Items)
        {
            var item = items.FirstOrDefault(i => i.Id == dto.Id)
                ?? throw new DomainException($"Item {dto.Id} não encontrado.");

            item.Update(dto.Sku, dto.Estampa, dto.Color, dto.Size, dto.Quantity);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id,
            list.FileName,
            list.UploadedAt,
            list.Status.ToString(),
            items.OrderBy(i => i.SortOrder)
                 .Select(i => new SeparationItemDto(
                     i.Id, i.Sku, i.Estampa, i.Color, i.Size, i.Quantity, i.SortOrder, i.DtfModelId))
                 .ToList()
        );
```

- [ ] **Step 5: Atualizar `GetSkuCodesQueryHandler.cs`**

Trocar:
```csharp
            .Select(c => new SkuCodeDto(c.Id, c.Code, c.Value, c.Category.ToString()))
```
Por:
```csharp
            .Select(c => new SkuCodeDto(c.Id, c.Code, c.Value, c.Category.ToString(), c.DtfModelId))
```

- [ ] **Step 6: Atualizar `GetSeparationListByIdQueryHandler.cs`**

Trocar:
```csharp
        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Color, i.Size, i.Quantity, i.SortOrder)).ToList()
        );
```
Por:
```csharp
        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Estampa, i.Color, i.Size, i.Quantity, i.SortOrder, i.DtfModelId)).ToList()
        );
```

- [ ] **Step 7: Atualizar o controller — `SeparationListsController.cs`**

Localizar (linha ~152):
```csharp
public record UpsertSkuCodeRequest(Guid? Id, string Code, string Value, string Category);
```
Trocar por:
```csharp
public record UpsertSkuCodeRequest(Guid? Id, string Code, string Value, string Category, Guid? DtfModelId);
```

Localizar (linha ~95):
```csharp
            var result = await mediator.Send(new UpsertSkuCodeCommand(request.Id, request.Code, request.Value, request.Category), ct);
```
Trocar por:
```csharp
            var result = await mediator.Send(new UpsertSkuCodeCommand(request.Id, request.Code, request.Value, request.Category, request.DtfModelId), ct);
```

> Nota: este Step 7 referencia o command `UpsertSkuCodeCommand` com 5 parâmetros, que só existe a partir da Task 9. Está incluído aqui porque mexe no mesmo arquivo de DTO de request — execute este Step 7 **junto com a Task 9** (ou adicione `Guid? DtfModelId` ao command já na Task 9 antes deste Step 7; a ordem entre Task 8 e 9 não importa desde que ambas sejam concluídas antes de compilar).

- [ ] **Step 8: Compilar tudo**

Run: `cd backend && dotnet build`
Expected: ainda falha em `UpsertSkuCodeCommand`/`UpsertSkuCodeCommandHandler.cs` (resolvido na Task 9) e em `UploadSeparationListCommandHandler.cs` se a Task 7 ainda não tiver sido feita. Confirmar que os erros restantes são exatamente esses dois pontos.

- [ ] **Step 9: Commit**

```bash
cd backend
git add src/Application/Separation src/API/Controllers/SeparationListsController.cs
git commit -m "feat: propaga Estampa e DtfModelId pelos DTOs e commands de separação"
```

---

## Task 9: `UpsertSkuCodeCommand` — suportar `DtfModelId`

**Files:**
- Modify: `backend/src/Application/Separation/Commands/UpsertSkuCode/UpsertSkuCodeCommand.cs`
- Modify: `backend/src/Application/Separation/Commands/UpsertSkuCode/UpsertSkuCodeCommandHandler.cs`

- [ ] **Step 1: Escrever o teste que falha primeiro**

Criar `backend/tests/Application.Tests/Separation/UpsertSkuCodeCommandHandlerTests.cs`:

```csharp
using FluentAssertions;
using SistemaTraction.Application.Separation.Commands.UpsertSkuCode;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Tests.Separation;

public class UpsertSkuCodeCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly UpsertSkuCodeCommandHandler _handler;

    public UpsertSkuCodeCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpsertSkuCodeCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_EstampaCategory_RequiresDtfModelId()
    {
        var command = new UpsertSkuCodeCommand(null, "MADT", "Made in Traction", "Estampa", null);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*modelo DTF*");
    }

    [Fact]
    public async Task Handle_EstampaCategory_WithDtfModelId_Saves()
    {
        var dtfModel = DtfModel.Create("Made in Traction", "Folha A3", 6, 25m);
        _context.DtfModels.Add(dtfModel);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new UpsertSkuCodeCommand(null, "MADT", "Made in Traction", "Estampa", dtfModel.Id);
        var dto = await _handler.Handle(command, CancellationToken.None);

        dto.Category.Should().Be("Estampa");
        dto.DtfModelId.Should().Be(dtfModel.Id);
    }

    [Fact]
    public async Task Handle_CorCategory_WithDtfModelId_Throws()
    {
        var command = new UpsertSkuCodeCommand(null, "BLK", "Preto", "Cor", Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Estampa*");
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Rodar o teste e confirmar que falha (não compila ainda)**

Run: `cd backend && dotnet test tests/Application.Tests --filter UpsertSkuCodeCommandHandlerTests`
Expected: erro de compilação — `UpsertSkuCodeCommand` não tem um 5º parâmetro `Guid?`.

- [ ] **Step 3: Atualizar `UpsertSkuCodeCommand.cs`**

```csharp
using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.UpsertSkuCode;

public record UpsertSkuCodeCommand(Guid? Id, string Code, string Value, string Category, Guid? DtfModelId)
    : IRequest<SkuCodeDto>;
```

- [ ] **Step 4: Atualizar `UpsertSkuCodeCommandHandler.cs`**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.UpsertSkuCode;

public class UpsertSkuCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpsertSkuCodeCommand, SkuCodeDto>
{
    public async Task<SkuCodeDto> Handle(UpsertSkuCodeCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SkuCodeCategory>(request.Category, out var category))
            throw new DomainException($"Categoria inválida: {request.Category}. Use: Modelo, Estampa, Cor, Tamanho.");

        SkuCode skuCode;

        if (request.Id.HasValue)
        {
            skuCode = await context.SkuCodes
                .FirstOrDefaultAsync(c => c.Id == request.Id.Value && !c.IsDeleted, cancellationToken)
                ?? throw new DomainException("Código SKU não encontrado.");

            skuCode.Update(request.Code, request.Value, category, request.DtfModelId);
        }
        else
        {
            skuCode = SkuCode.Create(request.Code, request.Value, category, request.DtfModelId);
            context.SkuCodes.Add(skuCode);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SkuCodeDto(skuCode.Id, skuCode.Code, skuCode.Value, skuCode.Category.ToString(), skuCode.DtfModelId);
    }
}
```

- [ ] **Step 5: Rodar os testes de novo**

Run: `cd backend && dotnet test tests/Application.Tests --filter UpsertSkuCodeCommandHandlerTests`
Expected: `Passed!` (3 testes).

- [ ] **Step 6: Confirmar que o controller já foi atualizado**

A Task 8 (Step 7) já atualizou `UpsertSkuCodeRequest` e a chamada a `UpsertSkuCodeCommand` em `SeparationListsController.cs` para 5 parâmetros. Se a Task 8 ainda não foi executada, faça-a agora antes de compilar (ela está descrita lá com o código exato).

- [ ] **Step 7: Compilar a solução inteira**

Run: `cd backend && dotnet build`
Expected: `Build succeeded`.

- [ ] **Step 8: Commit**

```bash
cd backend
git add src/Application/Separation/Commands/UpsertSkuCode tests/Application.Tests/Separation/UpsertSkuCodeCommandHandlerTests.cs src/API
git commit -m "feat: UpsertSkuCode aceita DtfModelId para categoria Estampa"
```

---

## Task 10: Teste do parser de SKU 4 segmentos

**Files:**
- Create: `backend/tests/Application.Tests/Separation/UploadSeparationListCommandHandlerTests.cs`

> Precisa de uma implementação fake de `IPdfParser` para controlar a entrada sem depender de um PDF real.

- [ ] **Step 1: Escrever o teste**

```csharp
using FluentAssertions;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.Commands.UploadSeparationList;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Tests.Separation;

public class FakePdfParser : IPdfParser
{
    private readonly ParseResult _result;
    public FakePdfParser(ParseResult result) => _result = result;
    public ParseResult Parse(Stream pdfStream, string fileName) => _result;
}

public class UploadSeparationListCommandHandlerTests : IDisposable
{
    private readonly TestApplicationDbContext _context;

    public UploadSeparationListCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task Handle_FourSegmentSku_ResolvesEstampaCorTamanhoAndLinksDtfModel()
    {
        var dtfModel = DtfModel.Create("Made in Traction", "Folha A3", 6, 25m);
        _context.DtfModels.Add(dtfModel);

        _context.SkuCodes.Add(SkuCode.Create("MADT", "Made in Traction", SkuCodeCategory.Estampa, dtfModel.Id));
        _context.SkuCodes.Add(SkuCode.Create("RED", "Vermelho", SkuCodeCategory.Cor));
        _context.SkuCodes.Add(SkuCode.Create("G", "G", SkuCodeCategory.Tamanho));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var parser = new FakePdfParser(new ParseResult(
            true,
            [new ParsedItem("REG-MADT-RED-G", "", "", 3, 0)],
            null));

        var handler = new UploadSeparationListCommandHandler(_context, parser);
        var command = new UploadSeparationListCommand(new MemoryStream(), "lista.pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Items.Should().ContainSingle();
        var item = result.Items[0];
        item.Sku.Should().Be("REG-MADT-RED-G");
        item.Estampa.Should().Be("Made in Traction");
        item.Color.Should().Be("Vermelho");
        item.Size.Should().Be("G");
        item.DtfModelId.Should().Be(dtfModel.Id);
    }

    [Fact]
    public async Task Handle_ThreeSegmentSku_LegacyFormat_NoEstampa()
    {
        _context.SkuCodes.Add(SkuCode.Create("BLK", "Preto", SkuCodeCategory.Cor));
        _context.SkuCodes.Add(SkuCode.Create("M", "M", SkuCodeCategory.Tamanho));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var parser = new FakePdfParser(new ParseResult(
            true,
            [new ParsedItem("BBL-BLK-M", "", "", 1, 0)],
            null));

        var handler = new UploadSeparationListCommandHandler(_context, parser);
        var command = new UploadSeparationListCommand(new MemoryStream(), "lista.pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        var item = result.Items[0];
        item.Estampa.Should().BeEmpty();
        item.Color.Should().Be("Preto");
        item.Size.Should().Be("M");
        item.DtfModelId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_FourSegmentSku_UnconfiguredEstampaCode_FallsBackToRawCode()
    {
        _context.SkuCodes.Add(SkuCode.Create("RED", "Vermelho", SkuCodeCategory.Cor));
        _context.SkuCodes.Add(SkuCode.Create("G", "G", SkuCodeCategory.Tamanho));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var parser = new FakePdfParser(new ParseResult(
            true,
            [new ParsedItem("REG-XYZ-RED-G", "", "", 1, 0)],
            null));

        var handler = new UploadSeparationListCommandHandler(_context, parser);
        var command = new UploadSeparationListCommand(new MemoryStream(), "lista.pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        var item = result.Items[0];
        item.Estampa.Should().Be("XYZ");
        item.DtfModelId.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
```

- [ ] **Step 2: Verificar a assinatura real de `UploadSeparationListCommand`**

Run: `grep -n "record UploadSeparationListCommand" backend/src/Application/Separation/Commands/UploadSeparationList/UploadSeparationListCommand.cs`
Ajustar a construção de `command` no teste acima para os parâmetros exatos (nomes/ordem) retornados — o teste assume `(Stream PdfStream, string FileName)` na mesma ordem usada pelo handler (`request.PdfStream`, `request.FileName`).

- [ ] **Step 3: Rodar os testes**

Run: `cd backend && dotnet test tests/Application.Tests --filter UploadSeparationListCommandHandlerTests`
Expected: `Passed!` (3 testes). Se falhar por causa de ordem de propriedades no `SeparationItemDto`, revisar Task 8 (a ordem precisa ser `Id, Sku, Estampa, Color, Size, Quantity, SortOrder, DtfModelId`).

- [ ] **Step 4: Commit**

```bash
cd backend
git add tests/Application.Tests/Separation/UploadSeparationListCommandHandlerTests.cs
git commit -m "test: cobre parsing de SKU 4 e 3 segmentos e vínculo com DtfModel"
```

---

## Task 11: Rodar a suíte completa de testes do backend

**Files:** nenhum (apenas verificação)

- [ ] **Step 1: Build completo**

Run: `cd backend && dotnet build`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 2: Testes completos**

Run: `cd backend && dotnet test`
Expected: todos os testes passam, incluindo os já existentes de `CancelCuttingOrderCommandHandlerTests` e `ReverseFinancialEntryCommandHandlerTests` (não relacionados, mas devem continuar verdes).

- [ ] **Step 3: Se algo quebrou, corrigir antes de seguir**

Se algum teste pré-existente falhar por causa da nova assinatura de `SeparationItemDto`/`SkuCodeDto`/`SeparationItem.Create`, localizar com:
```bash
grep -rln "SeparationItem.Create(\|new SeparationItemDto(\|new SkuCodeDto(" backend/src backend/tests
```
e atualizar cada ocorrência para a nova assinatura definida nas Tasks 3, 8 e 9.

---

## Task 12: Frontend — `types.ts`

**Files:**
- Modify: `frontend/src/features/separation/types.ts`

- [ ] **Step 1: Editar os tipos**

Substituir:
```ts
export interface SeparationItemDto {
  id: string
  sku: string
  color: string
  size: string
  quantity: number
  sortOrder: number
}
```
Por:
```ts
export interface SeparationItemDto {
  id: string
  sku: string
  estampa: string
  color: string
  size: string
  quantity: number
  sortOrder: number
  dtfModelId: string | null
}
```

Substituir:
```ts
export interface UpdateItemPayload {
  id: string
  sku: string
  color: string
  size: string
  quantity: number
}
```
Por:
```ts
export interface UpdateItemPayload {
  id: string
  sku: string
  estampa: string
  color: string
  size: string
  quantity: number
}
```

Substituir:
```ts
// SKU code categories — extensible for future additions
export type SkuCodeCategory = 'Modelo' | 'Cor' | 'Tamanho'

export interface SkuCodeDto {
  id: string
  code: string
  value: string
  category: SkuCodeCategory
}
```
Por:
```ts
// SKU code categories — extensible for future additions
export type SkuCodeCategory = 'Modelo' | 'Estampa' | 'Cor' | 'Tamanho'

export interface SkuCodeDto {
  id: string
  code: string
  value: string
  category: SkuCodeCategory
  dtfModelId: string | null
}
```

- [ ] **Step 2: Typecheck**

Run: `cd frontend && pnpm typecheck`
Expected: erros nos arquivos que usam esses tipos sem os novos campos (`SeparationListPage.tsx`, `SkuConfigPanel.tsx`, hooks) — esperado, corrigido nas próximas tasks. Anotar a lista de arquivos com erro para confirmar que bate com o que será editado.

- [ ] **Step 3: Commit**

```bash
cd frontend
git add src/features/separation/types.ts
git commit -m "feat: adiciona Estampa e DtfModelId aos tipos de separação"
```

---

## Task 13: Frontend — hooks (`useUpdateSeparationItems`, `useSkuCodes`)

**Files:**
- Ler e, se necessário, ajustar: `frontend/src/features/separation/hooks/useUpdateSeparationItems.ts`
- Modify: `frontend/src/features/separation/hooks/useSkuCodes.ts`

- [ ] **Step 1: Confirmar que `useUpdateSeparationItems.ts` não precisa de alteração**

O arquivo já usa o tipo `UpdateItemPayload[]` diretamente (sem payload duplicado):
```ts
import { useMutation } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { SeparationListDetail, UpdateItemPayload } from '../types'

export function useUpdateSeparationItems() {
  return useMutation({
    mutationFn: async ({ listId, items }: { listId: string; items: UpdateItemPayload[] }) => {
      const { data } = await api.put<SeparationListDetail>(`/separation-lists/${listId}/items`, { items })
      return data
    },
  })
}
```
Como `UpdateItemPayload` já ganhou `estampa: string` na Task 12, nenhuma mudança é necessária neste arquivo. Nenhum commit nesta etapa.

- [ ] **Step 2: Atualizar `useUpsertSkuCode` em `useSkuCodes.ts`**

Trocar:
```ts
export function useUpsertSkuCode() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      id?: string
      code: string
      value: string
      category: SkuCodeCategory
    }) => {
      const { data } = await api.post<SkuCodeDto>('/separation-lists/sku-codes', payload)
      return data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sku-codes'] }),
  })
}
```
Por:
```ts
export function useUpsertSkuCode() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      id?: string
      code: string
      value: string
      category: SkuCodeCategory
      dtfModelId?: string | null
    }) => {
      const { data } = await api.post<SkuCodeDto>('/separation-lists/sku-codes', payload)
      return data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sku-codes'] }),
  })
}
```

- [ ] **Step 3: Commit**

```bash
cd frontend
git add src/features/separation/hooks/useSkuCodes.ts
git commit -m "feat: useUpsertSkuCode aceita dtfModelId"
```

---

## Task 14: Frontend — tabela de revisão em `SeparationListPage.tsx`

**Files:**
- Modify: `frontend/src/features/separation/SeparationListPage.tsx:184-247` (bloco `review`)

- [ ] **Step 1: Adicionar coluna "Estampa" no cabeçalho da tabela**

Em `SeparationListPage.tsx`, dentro do `<thead>` do passo `review` (linhas ~186-194), trocar:
```tsx
              <thead className="bg-muted">
                <tr>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">SKU</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Modelo</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Cor</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Tam.</th>
                  <th className="px-2 py-2 text-center text-xs font-medium text-muted-foreground">Qtd</th>
                </tr>
              </thead>
```
Por:
```tsx
              <thead className="bg-muted">
                <tr>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">SKU</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Modelagem</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Estampa</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Cor</th>
                  <th className="px-2 py-2 text-left text-xs font-medium text-muted-foreground">Tam.</th>
                  <th className="px-2 py-2 text-center text-xs font-medium text-muted-foreground">Qtd</th>
                </tr>
              </thead>
```

- [ ] **Step 2: Adicionar célula "Estampa" no corpo da tabela**

Trocar o bloco `<tbody>` (linhas ~195-227):
```tsx
              <tbody>
                {editedItems.map((item, idx) => {
                  // Derive model from SKU (first segment before '-')
                  const modelCode = item.sku.split('-')[0] ?? ''
                  return (
                    <tr key={item.id} className={idx % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                      <td className="px-2 py-1">
                        <input value={item.sku}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, sku: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 font-mono" />
                      </td>
                      <td className="px-2 py-1">
                        <span className="text-xs font-mono text-muted-foreground">{modelCode || '—'}</span>
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.color}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, color: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1" />
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.size}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, size: e.target.value.toUpperCase() } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 uppercase" />
                      </td>
                      <td className="px-2 py-1 text-center">
                        <input type="number" min="1" value={item.quantity}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, quantity: parseInt(e.target.value) || 1 } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 text-center" />
                      </td>
                    </tr>
                  )
                })}
              </tbody>
```
Por:
```tsx
              <tbody>
                {editedItems.map((item, idx) => {
                  // Derive model from SKU (first segment before '-')
                  const modelCode = item.sku.split('-')[0] ?? ''
                  return (
                    <tr key={item.id} className={idx % 2 === 0 ? 'bg-card' : 'bg-muted/50'}>
                      <td className="px-2 py-1">
                        <input value={item.sku}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, sku: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 font-mono" />
                      </td>
                      <td className="px-2 py-1">
                        <span className="text-xs font-mono text-muted-foreground">{modelCode || '—'}</span>
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.estampa}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, estampa: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1" />
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.color}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, color: e.target.value } : i))}
                          className="w-full text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1" />
                      </td>
                      <td className="px-2 py-1">
                        <input value={item.size}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, size: e.target.value.toUpperCase() } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 uppercase" />
                      </td>
                      <td className="px-2 py-1 text-center">
                        <input type="number" min="1" value={item.quantity}
                          onChange={(e) => setEditedItems(p => p.map(i => i.id === item.id ? { ...i, quantity: parseInt(e.target.value) || 1 } : i))}
                          className="w-12 text-xs border-0 bg-transparent focus:outline-none focus:ring-1 focus:ring-ring rounded px-1 text-center" />
                      </td>
                    </tr>
                  )
                })}
              </tbody>
```

- [ ] **Step 3: Atualizar o botão "+ Linha" para incluir `estampa` e `dtfModelId` no item vazio**

Localizar (linha ~232-237):
```tsx
            <Button variant="outline" className="text-xs h-7 px-2"
              onClick={() => setEditedItems(p => [...p, {
                id: crypto.randomUUID(), sku: '', color: '', size: '', quantity: 1, sortOrder: p.length
              }])}>
              + Linha
            </Button>
```
Trocar por:
```tsx
            <Button variant="outline" className="text-xs h-7 px-2"
              onClick={() => setEditedItems(p => [...p, {
                id: crypto.randomUUID(), sku: '', estampa: '', color: '', size: '', quantity: 1, sortOrder: p.length, dtfModelId: null
              }])}>
              + Linha
            </Button>
```

- [ ] **Step 4: Typecheck**

Run: `cd frontend && pnpm typecheck`
Expected: erros restantes só em `SkuConfigPanel.tsx` (resolvido na Task 15). Confirmar que `SeparationListPage.tsx` não aparece mais na lista de erros.

- [ ] **Step 5: Commit**

```bash
cd frontend
git add src/features/separation/SeparationListPage.tsx
git commit -m "feat: exibe e edita coluna Estampa na revisão da lista de separação"
```

---

## Task 15: Frontend — `SkuConfigPanel.tsx` (categoria Estampa + select de DtfModel)

**Files:**
- Modify: `frontend/src/features/separation/components/SkuConfigPanel.tsx`

- [ ] **Step 1: Importar `useDtfModels` e ajustar constantes de categoria**

No topo do arquivo, trocar:
```tsx
import { useState } from 'react'
import { useSkuCodes, useUpsertSkuCode, useDeleteSkuCode } from '../hooks/useSkuCodes'
import type { SkuCodeCategory, SkuCodeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

// Extensible: add new categories here as needed in the future
const CATEGORIES: { value: SkuCodeCategory; label: string }[] = [
  { value: 'Modelo',  label: 'Modelo' },
  { value: 'Cor',     label: 'Cor' },
  { value: 'Tamanho', label: 'Tamanho' },
]

type CategoryBadgeVariant = 'primary' | 'warning' | 'success'
const CATEGORY_VARIANT: Record<SkuCodeCategory, CategoryBadgeVariant> = {
  Modelo:  'primary',
  Cor:     'warning',
  Tamanho: 'success',
}

const EMPTY_FORM = {
  code: '', value: '', category: 'Cor' as SkuCodeCategory,
}
```
Por:
```tsx
import { useState } from 'react'
import { useSkuCodes, useUpsertSkuCode, useDeleteSkuCode } from '../hooks/useSkuCodes'
import { useDtfModels } from '@/features/settings/dtf/hooks/useDtfModels'
import type { SkuCodeCategory, SkuCodeDto } from '../types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

// Extensible: add new categories here as needed in the future
const CATEGORIES: { value: SkuCodeCategory; label: string }[] = [
  { value: 'Modelo',  label: 'Modelo' },
  { value: 'Estampa', label: 'Estampa' },
  { value: 'Cor',     label: 'Cor' },
  { value: 'Tamanho', label: 'Tamanho' },
]

type CategoryBadgeVariant = 'primary' | 'info' | 'warning' | 'success'
const CATEGORY_VARIANT: Record<SkuCodeCategory, CategoryBadgeVariant> = {
  Modelo:  'primary',
  Estampa: 'info',
  Cor:     'warning',
  Tamanho: 'success',
}

const EMPTY_FORM = {
  code: '', value: '', category: 'Cor' as SkuCodeCategory, dtfModelId: null as string | null,
}
```

- [ ] **Step 2: Confirmar a variante `info` no componente `Badge`**

`frontend/src/components/ui/badge.tsx` já define a variante `info: 'border-info/20 bg-info/10 text-info'` em `badgeVariants`. Nenhuma alteração necessária nesse arquivo — `info` pode ser usada diretamente no Step 1.

- [ ] **Step 3: Atualizar `handleEdit` e `handleCategoryChange` para o novo campo**

Trocar:
```tsx
  function handleEdit(c: SkuCodeDto) {
    setEditingId(c.id)
    setForm({ code: c.code, value: c.value, category: c.category })
  }

  function handleCancel() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function handleCategoryChange(cat: SkuCodeCategory) {
    setForm(f => ({ ...f, category: cat, value: '' }))
  }

  function handleSave() {
    if (!form.code.trim() || !form.value.trim()) return

    upsert.mutate(
      { id: editingId ?? undefined, ...form },
      { onSuccess: () => { setEditingId(null); setForm(EMPTY_FORM) } },
    )
  }
```
Por:
```tsx
  function handleEdit(c: SkuCodeDto) {
    setEditingId(c.id)
    setForm({ code: c.code, value: c.value, category: c.category, dtfModelId: c.dtfModelId })
  }

  function handleCancel() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function handleCategoryChange(cat: SkuCodeCategory) {
    setForm(f => ({ ...f, category: cat, value: '', dtfModelId: null }))
  }

  const dtfModelsQuery = useDtfModels()

  function handleDtfModelChange(dtfModelId: string) {
    const model = dtfModelsQuery.data?.find(m => m.id === dtfModelId)
    setForm(f => ({ ...f, dtfModelId, value: model?.name ?? '' }))
  }

  function handleSave() {
    if (!form.code.trim()) return
    if (form.category === 'Estampa' && !form.dtfModelId) return
    if (form.category !== 'Estampa' && !form.value.trim()) return

    upsert.mutate(
      { id: editingId ?? undefined, code: form.code, value: form.value, category: form.category, dtfModelId: form.dtfModelId },
      { onSuccess: () => { setEditingId(null); setForm(EMPTY_FORM) } },
    )
  }
```

- [ ] **Step 4: Trocar o campo "Valor" por select de DtfModel quando categoria = Estampa**

Localizar o bloco do campo "Valor" (linhas ~101-117):
```tsx
          {/* Valor */}
          <div>
            <label className="text-xs text-muted-foreground mb-0.5 block">
              {form.category === 'Cor'     ? 'Nome da cor (deve coincidir com o estoque)' :
               form.category === 'Tamanho' ? 'Tamanho (ex: G, M, GG, G1)' : 'Valor (nome do modelo)'}
            </label>
            <Input
              value={form.value}
              onChange={(e) => setForm(f => ({ ...f, value: e.target.value }))}
              placeholder={
                form.category === 'Cor'     ? 'ex: Preto' :
                form.category === 'Tamanho' ? 'ex: G' : 'ex: Babylook'
              }
              className="text-sm"
            />
          </div>
        </div>

        {form.category === 'Cor' && (
          <p className="text-xs text-warning bg-warning/10 border border-warning/20 rounded px-2 py-1">
            O valor da Cor deve ser idêntico ao nome da cor cadastrada no estoque de camisetas.
          </p>
        )}
```
Trocar por:
```tsx
          {/* Valor */}
          <div>
            <label className="text-xs text-muted-foreground mb-0.5 block">
              {form.category === 'Cor'     ? 'Nome da cor (deve coincidir com o estoque)' :
               form.category === 'Tamanho' ? 'Tamanho (ex: G, M, GG, G1)' :
               form.category === 'Estampa' ? 'Modelo DTF vinculado' : 'Valor (nome do modelo)'}
            </label>
            {form.category === 'Estampa' ? (
              <select
                value={form.dtfModelId ?? ''}
                onChange={(e) => handleDtfModelChange(e.target.value)}
                className="flex h-9 w-full rounded-md border border-input bg-background px-3 text-sm text-foreground shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background"
              >
                <option value="">Selecione um modelo DTF</option>
                {(dtfModelsQuery.data ?? []).map(m => (
                  <option key={m.id} value={m.id}>{m.name}</option>
                ))}
              </select>
            ) : (
              <Input
                value={form.value}
                onChange={(e) => setForm(f => ({ ...f, value: e.target.value }))}
                placeholder={
                  form.category === 'Cor'     ? 'ex: Preto' :
                  form.category === 'Tamanho' ? 'ex: G' : 'ex: Babylook'
                }
                className="text-sm"
              />
            )}
          </div>
        </div>

        {form.category === 'Cor' && (
          <p className="text-xs text-warning bg-warning/10 border border-warning/20 rounded px-2 py-1">
            O valor da Cor deve ser idêntico ao nome da cor cadastrada no estoque de camisetas.
          </p>
        )}
        {form.category === 'Estampa' && (dtfModelsQuery.data ?? []).length === 0 && (
          <p className="text-xs text-warning bg-warning/10 border border-warning/20 rounded px-2 py-1">
            Nenhum modelo DTF cadastrado. Cadastre em Configurações → DTF antes de criar este código.
          </p>
        )}
```

- [ ] **Step 5: Atualizar o botão "Salvar" para considerar a validação de Estampa**

Localizar:
```tsx
          <Button
            onClick={handleSave}
            disabled={upsert.isPending || !form.code.trim() || !form.value.trim()}
            className="text-sm h-8 px-4"
          >
```
Trocar por:
```tsx
          <Button
            onClick={handleSave}
            disabled={
              upsert.isPending ||
              !form.code.trim() ||
              (form.category === 'Estampa' ? !form.dtfModelId : !form.value.trim())
            }
            className="text-sm h-8 px-4"
          >
```

- [ ] **Step 6: Atualizar texto de ajuda no topo do painel**

Localizar:
```tsx
        <p className="text-xs text-muted-foreground mt-0.5">
          Defina o significado de cada parte do SKU (ex:{' '}
          <code className="bg-muted px-1 rounded">BBL-BLK-M</code>).
          Formato: <strong>MODELO-COR-TAMANHO</strong>. Após configurar, o upload do PDF preencherá Cor e Tamanho automaticamente.
        </p>
```
Trocar por:
```tsx
        <p className="text-xs text-muted-foreground mt-0.5">
          Defina o significado de cada parte do SKU (ex:{' '}
          <code className="bg-muted px-1 rounded">REG-MADT-RED-G</code>).
          Formato: <strong>MODELAGEM-ESTAMPA-COR-TAMANHO</strong>. SKUs com 3 partes (formato antigo, sem Estampa) continuam funcionando como MODELO-COR-TAMANHO.
          Após configurar, o upload do PDF preencherá Estampa, Cor e Tamanho automaticamente.
        </p>
```

- [ ] **Step 7: Atualizar `SkuPreview` para 4 segmentos**

Localizar a função `SkuPreview`:
```tsx
function SkuPreview({ codes }: { codes: SkuCodeDto[] }) {
  const [sku, setSku] = useState('BBL-BLK-M')

  const byCode = new Map(codes.map(c => [c.code.toUpperCase(), c]))

  // SKU format: MODELO-COR-TAMANHO (3 segments)
  const parts = sku.toUpperCase().split('-').filter(Boolean)

  const resolved = parts.map((part, index) => {
    const match = byCode.get(part)
    // Infer expected category by position
    const expectedCategory: SkuCodeCategory | null =
      index === 0 ? 'Modelo' : index === 1 ? 'Cor' : index === 2 ? 'Tamanho' : null

    if (match) {
      return { part, code: match, display: match.value }
    }
    return { part, code: null, expectedCategory, display: null }
  })
```
Trocar por:
```tsx
function SkuPreview({ codes }: { codes: SkuCodeDto[] }) {
  const [sku, setSku] = useState('REG-MADT-RED-G')

  const byCode = new Map(codes.map(c => [c.code.toUpperCase(), c]))

  // SKU format: MODELAGEM-ESTAMPA-COR-TAMANHO (4 segmentos) ou legado MODELO-COR-TAMANHO (3 segmentos)
  const parts = sku.toUpperCase().split('-').filter(Boolean)
  const isLegacyFormat = parts.length === 3

  const resolved = parts.map((part, index) => {
    const match = byCode.get(part)
    // Infer expected category by position
    const expectedCategory: SkuCodeCategory | null = isLegacyFormat
      ? (index === 0 ? 'Modelo' : index === 1 ? 'Cor' : index === 2 ? 'Tamanho' : null)
      : (index === 0 ? 'Modelo' : index === 1 ? 'Estampa' : index === 2 ? 'Cor' : index === 3 ? 'Tamanho' : null)

    if (match) {
      return { part, code: match, display: match.value }
    }
    return { part, code: null, expectedCategory, display: null }
  })
```

- [ ] **Step 8: Atualizar o placeholder do input de preview**

Localizar:
```tsx
        <input
          value={sku}
          onChange={(e) => setSku(e.target.value)}
          className="font-mono text-xs border border-input rounded px-2 py-1 w-44 bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          placeholder="BBL-BLK-M"
        />
```
Trocar por:
```tsx
        <input
          value={sku}
          onChange={(e) => setSku(e.target.value)}
          className="font-mono text-xs border border-input rounded px-2 py-1 w-44 bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          placeholder="REG-MADT-RED-G"
        />
```

- [ ] **Step 9: Atualizar `getBadgeClasses` para a variante `info`**

Localizar:
```tsx
function getBadgeClasses(variant: CategoryBadgeVariant): string {
  switch (variant) {
    case 'primary': return 'bg-primary/10 text-primary border border-primary/20'
    case 'warning': return 'bg-warning/10 text-warning border border-warning/20'
    case 'success': return 'bg-success/10 text-success border border-success/20'
  }
}
```
Trocar por:
```tsx
function getBadgeClasses(variant: CategoryBadgeVariant): string {
  switch (variant) {
    case 'primary': return 'bg-primary/10 text-primary border border-primary/20'
    case 'info':    return 'bg-info/10 text-info border border-info/20'
    case 'warning': return 'bg-warning/10 text-warning border border-warning/20'
    case 'success': return 'bg-success/10 text-success border border-success/20'
  }
}
```

- [ ] **Step 10: Typecheck e lint**

Run: `cd frontend && pnpm typecheck && pnpm lint`
Expected: `0 erros`.

- [ ] **Step 11: Commit**

```bash
cd frontend
git add src/features/separation/components/SkuConfigPanel.tsx
git commit -m "feat: Config. SKU ganha categoria Estampa vinculada a modelo DTF"
```

---

## Task 16: Verificação manual end-to-end

**Files:** nenhum (apenas verificação)

- [ ] **Step 1: Subir backend e frontend**

Run: `cd backend && dotnet run --project src/API` (em um terminal)
Run: `cd frontend && pnpm dev` (em outro terminal)

- [ ] **Step 2: Cadastrar um código de Estampa**

Na UI, abrir Separação → Config. SKU → categoria "Estampa" → selecionar um `DtfModel` já cadastrado (criar um em Configurações → DTF antes, se não houver nenhum) → código, ex: `MADT` → salvar. Confirmar que aparece na tabela com o nome do modelo DTF como valor.

- [ ] **Step 3: Cadastrar Cor e Tamanho correspondentes**

Cadastrar `RED` → Cor → `Vermelho`, e `G` → Tamanho → `G` (ou os valores que baterem com o estoque de camisetas existente).

- [ ] **Step 4: Subir o PDF de teste**

Em Separação → Listas → Nova lista, subir o PDF `C:\Users\andre\Downloads\Imprimir - UpSeller test.pdf` (o mesmo enviado nesta conversa). Na tela de revisão, confirmar:
- coluna **Modelagem** mostra `REG` para os itens `REG-...`.
- coluna **Estampa** mostra o nome do modelo DTF para os SKUs com código cadastrado (ex: `MADT` → "Made in Traction") e o código bruto (ex: `ANGE`, `REDR`, `TRAO`) para os não cadastrados.
- coluna **Cor**/**Tam.** preenchidas corretamente para os SKUs de 4 segmentos.
- itens com SKU não-padrão (`48581264638184`, `CAM0015BASPREPG`) aparecem com `?` em Cor/Tamanho e Estampa vazia, sem quebrar a tela.

- [ ] **Step 5: Confirmar que a checagem de estoque não foi afetada**

Avançar para "Verificar estoque" e confirmar que o agrupamento por Modelo/Cor/Tamanho continua funcionando como antes (sem Estampa entrando na conta).

- [ ] **Step 6: Reportar quaisquer divergências encontradas e corrigi-las antes de finalizar**
