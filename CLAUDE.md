# Sistema Traction — Gestão de E-commerce de Camisetas

## Visão geral
Sistema web para controle de estoque, pedidos com fornecedores e operações de uma loja de camisetas online.

## Stack
- **Frontend:** React + TypeScript (Vite)
- **Backend:** .NET C# (ASP.NET Core Web API)
- **Banco de dados:** SQL Server

## Estrutura de pastas (planejada)
```
/
├── frontend/          # React + TypeScript
├── backend/           # .NET C# API
└── CLAUDE.md
```

## Convenções
- Frontend: componentes em PascalCase, hooks em camelCase com prefixo `use`
- Backend: seguir convenções padrão C# (PascalCase para classes/métodos, camelCase para variáveis)
- Commits em português
- Sem comentários desnecessários no código

## Comandos principais

### Frontend
```bash
cd frontend
npm install       # instalar dependências
npm run dev       # servidor de desenvolvimento
npm run build     # build de produção
npm run lint      # lint
```

### Backend
```bash
cd backend
dotnet restore    # restaurar pacotes NuGet
dotnet run        # rodar API
dotnet build      # compilar
dotnet test       # testes
dotnet ef migrations add <Nome>   # nova migration
dotnet ef database update         # aplicar migrations
```

## Status do projeto
Fase inicial — estrutura ainda não criada.
