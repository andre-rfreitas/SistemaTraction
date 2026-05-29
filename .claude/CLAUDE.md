# StockShirt — Sistema de gestão de estoque e pedidos

Sistema web para controle de estoque, pedidos a fornecedores e operação de e-commerce de camisetas. Monorepo com frontend React/TypeScript e backend .NET C# (Clean Architecture).

## Stack aprovada

### Frontend (`/frontend`)
- React 18 + TypeScript (strict mode)
- Vite como build tool
- TanStack Query para data fetching e cache
- React Hook Form + Zod para formulários e validação
- Zustand para estado global (apenas quando necessário)
- Tailwind CSS para estilos
- shadcn/ui para componentes base
- Vitest + Testing Library para testes
- pnpm como package manager

**NÃO introduzir:** Redux, Axios, styled-components, Material UI, Ant Design, Emotion.

### Backend (`/backend`)
- .NET 8, C#
- Clean Architecture (Domain / Application / Infrastructure / API)
- Entity Framework Core com SQL Server
- MediatR para CQRS (Commands e Queries)
- FluentValidation para validação
- Serilog para logs
- xUnit + FluentAssertions para testes

**NÃO introduzir:** Dapper direto nas camadas de Application/Domain, lógica de negócio em Controllers.

## Estrutura de pastas

```
/
├── frontend/
│   ├── src/
│   │   ├── components/     # componentes reutilizáveis
│   │   ├── features/       # módulos de domínio (stock, orders, suppliers)
│   │   ├── lib/            # utils, api client, store
│   │   └── pages/          # rotas principais
│   └── vite.config.ts
├── backend/
│   ├── src/
│   │   ├── Domain/         # entidades, value objects, enums
│   │   ├── Application/    # use cases, commands, queries, DTOs
│   │   ├── Infrastructure/ # EF Core, repositórios, serviços externos
│   │   └── API/            # controllers, middleware, DI
│   └── tests/
└── docs/                   # decisões de arquitetura (ADRs)
```

## Módulos do sistema
- **Estoque** — produtos (camisetas), tamanhos, cores, SKUs, movimentações
- **Fornecedores** — cadastro, histórico de pedidos, lead time
- **Pedidos a fornecedores** — criação, acompanhamento de status, recebimento
- **Pedidos de clientes** — integração futura; por ora apenas consulta de estoque
- **Relatórios** — posição de estoque, giro, ponto de reposição

## Convenções de código

### Frontend
- Componentes: PascalCase, arquivos `.tsx`
- Hooks customizados: prefixo `use`, ex: `useStockItems`
- Mutations TanStack Query sempre em hooks separados em `features/*/hooks/`
- Formulários sempre com React Hook Form + schema Zod exportado em `schemas/`
- Componentes max 200 linhas — extrair sub-componentes se ultrapassar

### Backend
- Commands terminam em `Command`, ex: `CreatePurchaseOrderCommand`
- Queries terminam em `Query`, ex: `GetStockItemsQuery`
- Handlers nunca acessam DbContext diretamente — usar repositórios
- Entidades de domínio sem setters públicos — mutação só via métodos
- IDs usam `Guid` como tipo no domínio

## Comandos do projeto

### Frontend
```bash
cd frontend
pnpm dev           # servidor de dev (porta 5173)
pnpm build         # build de produção
pnpm test          # testes com Vitest
pnpm lint          # ESLint + Prettier check
pnpm typecheck     # tsc --noEmit
```

Sempre rodar `pnpm typecheck && pnpm lint` antes de finalizar.

### Backend
```bash
cd backend
dotnet build                           # build completo
dotnet test                            # todos os testes
dotnet run --project src/API           # servidor de dev (porta 5000)
dotnet ef migrations add <Nome>        # nova migration
dotnet ef database update              # aplicar migrations
```

Sempre rodar `dotnet build && dotnet test` antes de finalizar.

## Regras de negócio críticas
- Estoque nunca pode ficar negativo — validar em domínio, não só na API
- SKU é único por produto+tamanho+cor — constraint no banco E no domínio
- Pedido a fornecedor só pode ser cancelado se status = Rascunho ou Enviado
- Movimentação de estoque é append-only — nunca atualizar, só inserir nova movimentação

## Banco de dados
- SQL Server (dev: LocalDB, prod: Azure SQL)
- Migrations gerenciadas via EF Core — nunca alterar banco manualmente
- Todas as tabelas têm: Id (Guid PK), CreatedAt, UpdatedAt, IsDeleted (soft delete)

## Quando compactar contexto

Ao usar `/compact`, sempre preservar:
- Lista de arquivos criados/modificados na sessão
- Decisões de arquitetura tomadas e motivo
- Pendências e próximos passos
- Qualquer constraint de negócio descoberta durante a sessão
