# Azure Deployment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publicar o SistemaTraction no Azure para acesso de qualquer lugar, com deploy automático via GitHub Actions.

**Architecture:** Frontend React hospedado no Azure Static Web Apps (free); backend .NET 9 hospedado no Azure App Service B1 (~R$70/mês); banco SQL Server no Azure SQL Database Basic (~R$25/mês). Deploy automático disparado por push na branch `main`.

**Tech Stack:** .NET 9, React 19 + Vite, Azure App Service, Azure Static Web Apps, Azure SQL Database, GitHub Actions.

---

## Mapa de Arquivos

| Arquivo | Ação | Responsabilidade |
|---|---|---|
| `backend/src/API/Program.cs` | Modificar | Ler origens CORS de configuração (não hardcoded) |
| `backend/src/API/appsettings.Production.json` | Criar | Config de produção (CORS origin, log level) |
| `frontend/src/lib/api.ts` | Modificar | Usar `VITE_API_URL` env var em produção |
| `frontend/.env.example` | Criar | Documentar variáveis de ambiente necessárias |
| `frontend/public/staticwebapp.config.json` | Criar | Roteamento SPA no Azure Static Web Apps |
| `.github/workflows/backend-deploy.yml` | Criar | CI/CD do backend para Azure App Service |
| `.github/workflows/frontend-deploy.yml` | Criar | CI/CD do frontend para Azure Static Web Apps |
| `docs/azure-setup.md` | Criar | Guia passo a passo de criação dos recursos no Azure |

---

## Task 1: Backend — CORS configurável por ambiente

**Files:**
- Modify: `backend/src/API/Program.cs`
- Create: `backend/src/API/appsettings.Production.json`

- [ ] **Step 1: Atualizar Program.cs para ler CORS da configuração**

Substituir o bloco CORS atual (linhas 21-26):

```csharp
// ANTES (hardcoded):
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

Por:

```csharp
// DEPOIS (lê da configuração):
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration["AllowedCorsOrigins"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? ["http://localhost:5173"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

- [ ] **Step 2: Criar appsettings.Production.json**

Criar `backend/src/API/appsettings.Production.json`:

```json
{
  "AllowedCorsOrigins": "https://NOME-DO-APP.azurestaticapps.net",
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Error",
        "System": "Error"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

> **Nota:** `NOME-DO-APP` será substituído pelo nome real após criar o Static Web App no Azure (Task 6 do guia). A connection string NÃO vai neste arquivo — será configurada como variável de ambiente no Azure App Service (mais seguro).

- [ ] **Step 3: Verificar que o build ainda passa**

```bash
cd backend
dotnet build
```

Esperado: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add backend/src/API/Program.cs backend/src/API/appsettings.Production.json
git commit -m "config: CORS configurável por ambiente para deploy Azure"
```

---

## Task 2: Frontend — URL da API via variável de ambiente

**Files:**
- Modify: `frontend/src/lib/api.ts`
- Create: `frontend/.env.example`

- [ ] **Step 1: Atualizar api.ts para usar variável de ambiente**

Substituir o conteúdo de `frontend/src/lib/api.ts`:

```typescript
import axios from 'axios'

const baseURL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

export const api = axios.create({
  baseURL,
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

**Como funciona:**
- Em desenvolvimento (`pnpm dev`): `VITE_API_URL` não está definido → usa `/api` → proxy do Vite redireciona para `localhost:5000`. Comportamento atual inalterado.
- Em produção (Azure): `VITE_API_URL=https://sistema-traction-api.azurewebsites.net` → usa `https://sistema-traction-api.azurewebsites.net/api`. Chamadas vão direto ao backend Azure.

- [ ] **Step 2: Criar .env.example**

Criar `frontend/.env.example`:

```env
# URL do backend sem barra no final
# Em dev: deixar vazio (usa proxy do Vite)
# Em produção: URL do Azure App Service
VITE_API_URL=https://sistema-traction-api.azurewebsites.net
```

- [ ] **Step 3: Verificar typecheck e build do frontend**

```bash
cd frontend
pnpm typecheck
pnpm build
```

Esperado: sem erros de TypeScript, `dist/` gerado.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/lib/api.ts frontend/.env.example
git commit -m "config: URL da API via VITE_API_URL para deploy Azure"
```

---

## Task 3: Frontend — Configuração de roteamento SPA no Azure

**Files:**
- Create: `frontend/public/staticwebapp.config.json`

O Azure Static Web Apps precisa saber que é uma SPA — senão, acessar uma rota direta (ex: `/pedidos`) retorna 404 pois tenta buscar o arquivo no servidor.

- [ ] **Step 1: Criar staticwebapp.config.json**

Criar `frontend/public/staticwebapp.config.json`:

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/api/*", "/*.{css,js,png,svg,ico,json,woff,woff2,ttf}"]
  },
  "globalHeaders": {
    "X-Content-Type-Options": "nosniff",
    "X-Frame-Options": "SAMEORIGIN",
    "Referrer-Policy": "strict-origin-when-cross-origin"
  }
}
```

- [ ] **Step 2: Verificar que o arquivo vai para o dist**

```bash
cd frontend
pnpm build
```

Verificar que `dist/staticwebapp.config.json` existe após o build (Vite copia tudo de `public/` para `dist/`).

- [ ] **Step 3: Commit**

```bash
git add frontend/public/staticwebapp.config.json
git commit -m "config: adicionar staticwebapp.config.json para roteamento SPA no Azure"
```

---

## Task 4: GitHub Actions — Deploy do backend

**Files:**
- Create: `.github/workflows/backend-deploy.yml`

- [ ] **Step 1: Criar pasta e arquivo de workflow**

```bash
mkdir -p .github/workflows
```

Criar `.github/workflows/backend-deploy.yml`:

```yaml
name: Deploy Backend → Azure App Service

on:
  push:
    branches: [main]
    paths:
      - 'backend/**'
      - '.github/workflows/backend-deploy.yml'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET 9
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore
        run: dotnet restore backend/src/API/SistemaTraction.API.csproj

      - name: Build
        run: dotnet build backend/src/API/SistemaTraction.API.csproj --configuration Release --no-restore

      - name: Test
        run: dotnet test backend/tests/Application.Tests/SistemaTraction.Application.Tests.csproj --configuration Release --no-build --verbosity normal

      - name: Publish
        run: dotnet publish backend/src/API/SistemaTraction.API.csproj --configuration Release --no-build --output ./publish

      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'sistema-traction-api'
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

> **Nota:** `app-name: 'sistema-traction-api'` deve bater com o nome exato que você der ao App Service no Azure. O secret `AZURE_WEBAPP_PUBLISH_PROFILE` é criado no Task 6 do guia de setup.

- [ ] **Step 2: Commit**

```bash
git add .github/workflows/backend-deploy.yml
git commit -m "ci: workflow de deploy do backend para Azure App Service"
```

---

## Task 5: GitHub Actions — Deploy do frontend

**Files:**
- Create: `.github/workflows/frontend-deploy.yml`

- [ ] **Step 1: Criar arquivo de workflow**

Criar `.github/workflows/frontend-deploy.yml`:

```yaml
name: Deploy Frontend → Azure Static Web Apps

on:
  push:
    branches: [main]
    paths:
      - 'frontend/**'
      - '.github/workflows/frontend-deploy.yml'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Setup pnpm
        uses: pnpm/action-setup@v4
        with:
          version: latest

      - name: Install dependencies
        working-directory: frontend
        run: pnpm install --frozen-lockfile

      - name: Build
        working-directory: frontend
        run: pnpm build
        env:
          VITE_API_URL: ${{ secrets.VITE_API_URL }}

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: 'upload'
          app_location: 'frontend'
          output_location: 'dist'
          skip_app_build: true
```

- [ ] **Step 2: Commit**

```bash
git add .github/workflows/frontend-deploy.yml
git commit -m "ci: workflow de deploy do frontend para Azure Static Web Apps"
```

---

## Task 6: Guia de setup do Azure

**Files:**
- Create: `docs/azure-setup.md`

- [ ] **Step 1: Criar arquivo de documentação**

Criar `docs/azure-setup.md`:

```markdown
# Setup Azure — SistemaTraction

Guia passo a passo para criar os recursos no Azure e configurar o deploy automático.

## Pré-requisitos

- Conta Azure (criar em portal.azure.com — tem R$900 de crédito grátis por 30 dias para novos usuários)
- Azure CLI instalado: https://learn.microsoft.com/pt-br/cli/azure/install-azure-cli
- Repositório no GitHub com o código do projeto
- Git configurado localmente

---

## 1. Criar Resource Group

No portal.azure.com ou via CLI:

```bash
az login
az group create --name rg-sistema-traction --location brazilsouth
```

---

## 2. Criar Azure SQL Database

**Via portal:**
1. Buscar "SQL databases" → Criar
2. Assinatura: sua assinatura
3. Resource group: `rg-sistema-traction`
4. Database name: `sistema-traction-db`
5. Server: Criar novo → nome único (ex: `sistema-traction-sql`)
6. Autenticação: SQL authentication → definir usuário e senha fortes
7. Compute + storage: clique em "Configure database" → Basic (5 DTUs, ~R$25/mês)
8. Networking: Allow Azure services = Yes
9. Criar

**Após criação, copiar a connection string:**
- Portal → SQL Database → Connection strings → ADO.NET
- Exemplo: `Server=tcp:sistema-traction-sql.database.windows.net,1433;Database=sistema-traction-db;User ID=adminuser;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=False;`

---

## 3. Criar Azure App Service (backend .NET)

**Via portal:**
1. Buscar "App Services" → Criar
2. Resource group: `rg-sistema-traction`
3. Name: `sistema-traction-api` ← este nome deve bater com o workflow do GitHub Actions
4. Publish: Code
5. Runtime stack: .NET 9
6. OS: Linux
7. Region: Brazil South
8. Pricing plan: Basic B1 (~R$70/mês)
9. Criar

**Configurar variáveis de ambiente (App Settings):**
- Portal → App Service → Configuration → Application settings → New application setting

Adicionar:
| Nome | Valor |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=tcp:sistema-traction-sql...` (string completa do passo 2) |
| `AllowedCorsOrigins` | `https://NOME-DO-APP.azurestaticapps.net` (preencher após criar o Static Web App) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

Clicar em Save.

**Baixar Publish Profile:**
- Portal → App Service → Overview → Get publish profile → Baixa um arquivo `.PublishSettings`
- No GitHub: Settings → Secrets and variables → Actions → New repository secret
  - Nome: `AZURE_WEBAPP_PUBLISH_PROFILE`
  - Valor: conteúdo do arquivo `.PublishSettings` (abrir no bloco de notas e copiar tudo)

---

## 4. Criar Azure Static Web Apps (frontend React)

**Via portal:**
1. Buscar "Static Web Apps" → Criar
2. Resource group: `rg-sistema-traction`
3. Name: qualquer nome (ex: `sistema-traction`)
4. Plan type: Free
5. Region: East US 2
6. Source: GitHub → autorizar → selecionar repositório e branch `main`
7. Build details:
   - App location: `/frontend`
   - Output location: `dist`
   - Deixar API location vazio
8. Criar

**Isso vai criar automaticamente um arquivo de workflow no seu repositório.** Deletar esse arquivo gerado automaticamente pois já temos o nosso em `.github/workflows/frontend-deploy.yml`.

**Pegar o token de deploy:**
- Portal → Static Web App → Manage deployment token → Copiar
- No GitHub: Settings → Secrets → New secret
  - Nome: `AZURE_STATIC_WEB_APPS_API_TOKEN`
  - Valor: token copiado

**Após criar, copiar a URL gerada** (ex: `https://sistema-traction-abc123.azurestaticapps.net`) e:
1. Atualizar `backend/src/API/appsettings.Production.json` com essa URL no `AllowedCorsOrigins`
2. Atualizar a variável de ambiente `AllowedCorsOrigins` no App Service

---

## 5. Configurar secrets do GitHub

No GitHub: Settings → Secrets and variables → Actions

| Secret | Como obter |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | App Service → Get publish profile |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Static Web App → Manage deployment token |
| `VITE_API_URL` | `https://sistema-traction-api.azurewebsites.net` (sem barra no final) |

---

## 6. Fazer o primeiro deploy

```bash
# Garantir que está na main e tudo commitado
git push origin main
```

Isso vai disparar os dois workflows:
- Backend: `Deploy Backend → Azure App Service`
- Frontend: `Deploy Frontend → Azure Static Web Apps`

Acompanhar em: GitHub → Actions → ver os jobs rodando.

---

## 7. Verificar

1. Abrir `https://sistema-traction-api.azurewebsites.net/scalar/v1` — deve aparecer a documentação da API
2. Abrir `https://sistema-traction-NOME.azurestaticapps.net` — deve carregar o sistema

---

## Custos estimados mensais

| Recurso | Plano | Custo (R$) |
|---|---|---|
| Azure App Service | B1 Basic | ~R$70 |
| Azure SQL Database | Basic 5 DTU | ~R$25 |
| Azure Static Web Apps | Free | R$0 |
| **Total** | | **~R$95/mês** |

---

## Domínio personalizado (opcional)

Após o sistema estar funcionando, é possível configurar um domínio próprio (ex: `sistema.minhaempresa.com.br`) em ambos os recursos no portal Azure → Custom Domains, sem custo adicional.
```

- [ ] **Step 2: Commit**

```bash
git add docs/azure-setup.md
git commit -m "docs: guia passo a passo de setup no Azure"
```

---

## Checklist final antes de ir ao ar

- [ ] `appsettings.Production.json` tem a URL correta do Static Web App no `AllowedCorsOrigins`
- [ ] Variável `AllowedCorsOrigins` no App Service configuration aponta para a URL correta do Static Web App
- [ ] Connection string do Azure SQL está configurada no App Service (não no código)
- [ ] Os 3 secrets estão configurados no GitHub: `AZURE_WEBAPP_PUBLISH_PROFILE`, `AZURE_STATIC_WEB_APPS_API_TOKEN`, `VITE_API_URL`
- [ ] `app-name` no workflow `backend-deploy.yml` bate com o nome do App Service criado no Azure
- [ ] Arquivo de workflow gerado automaticamente pelo Azure ao criar o Static Web App foi deletado
