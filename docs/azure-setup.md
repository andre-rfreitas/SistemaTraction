# Setup Azure — SistemaTraction

Guia passo a passo para criar os recursos no Azure e configurar o deploy automático.

## Pré-requisitos

- Conta Azure (criar em portal.azure.com — tem R$900 de crédito grátis por 30 dias para novos usuários)
- Repositório no GitHub com o código do projeto
- Git configurado localmente

---

## 1. Criar Resource Group

Em portal.azure.com → buscar "Resource groups" → Criar:
- Name: `rg-sistema-traction`
- Region: Brazil South

---

## 2. Criar Azure SQL Database

Portal → buscar "SQL databases" → Criar:

| Campo | Valor |
|---|---|
| Resource group | `rg-sistema-traction` |
| Database name | `sistema-traction-db` |
| Server | Criar novo: nome único (ex: `sistema-traction-sql`), SQL authentication, definir usuário e senha fortes |
| Compute + storage | Clique em "Configure database" → **Basic (5 DTUs)** |
| Networking | Allow Azure services and resources to access this server = **Yes** |

Após criação:
- Portal → SQL Database → **Connection strings** → ADO.NET → copiar a string
- Ela terá o formato: `Server=tcp:sistema-traction-sql.database.windows.net,1433;Database=sistema-traction-db;User ID=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;`

---

## 3. Criar Azure App Service (backend .NET)

Portal → buscar "App Services" → Criar:

| Campo | Valor |
|---|---|
| Resource group | `rg-sistema-traction` |
| Name | `sistema-traction-api` ← **deve bater exatamente com o workflow do GitHub Actions** |
| Publish | Code |
| Runtime stack | **.NET 9** |
| OS | Linux |
| Region | Brazil South |
| Pricing plan | **Basic B1** (~R$70/mês) |

### Configurar variáveis de ambiente (App Settings)

Portal → App Service → **Configuration** → Application settings → New application setting:

| Nome | Valor |
|---|---|
| `ConnectionStrings__DefaultConnection` | String completa do passo 2 (com usuário e senha) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `AllowedCorsOrigins` | Preencher após criar o Static Web App (passo 4) |

Clicar em **Save**.

### Baixar Publish Profile

Portal → App Service → Overview → **Get publish profile** → baixa arquivo `.PublishSettings`

No GitHub: **Settings → Secrets and variables → Actions → New repository secret**
- Nome: `AZURE_WEBAPP_PUBLISH_PROFILE`
- Valor: conteúdo do arquivo `.PublishSettings` (abrir no bloco de notas e copiar tudo)

---

## 4. Criar Azure Static Web Apps (frontend React)

Portal → buscar "Static Web Apps" → Criar:

| Campo | Valor |
|---|---|
| Resource group | `rg-sistema-traction` |
| Name | qualquer (ex: `sistema-traction`) |
| Plan type | **Free** |
| Region | East US 2 |
| Source | GitHub → autorizar → selecionar repositório e branch `main` |
| App location | `/frontend` |
| Output location | `dist` |
| API location | (deixar vazio) |

Criar.

**Após criar:**
- O Azure vai criar um workflow automático no seu repositório — **deletar esse arquivo** (nosso workflow em `.github/workflows/frontend-deploy.yml` já faz isso)
- Copiar a **URL gerada** (ex: `https://sistema-traction-abc123.azurestaticapps.net`)

### Pegar o token de deploy

Portal → Static Web App → **Manage deployment token** → copiar

No GitHub: Settings → Secrets → New secret
- Nome: `AZURE_STATIC_WEB_APPS_API_TOKEN`
- Valor: token copiado

---

## 5. Atualizar CORS com a URL real do Static Web App

Agora que você tem a URL do Static Web App (ex: `https://sistema-traction-abc123.azurestaticapps.net`):

**No código** — editar `backend/src/API/appsettings.Production.json`:
```json
{
  "AllowedCorsOrigins": "https://sistema-traction-abc123.azurestaticapps.net"
}
```

**No Azure** — App Service → Configuration → Application settings:
- `AllowedCorsOrigins` = `https://sistema-traction-abc123.azurestaticapps.net`

---

## 6. Configurar todos os secrets do GitHub

GitHub: **Settings → Secrets and variables → Actions**

| Secret | Como obter |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | App Service → Get publish profile (passo 3) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Static Web App → Manage deployment token (passo 4) |
| `VITE_API_URL` | `https://sistema-traction-api.azurewebsites.net` (sem barra no final) |

---

## 7. Fazer o primeiro deploy

```bash
git add .
git commit -m "config: preparação para deploy Azure"
git push origin main
```

Isso dispara os dois workflows. Acompanhar em: **GitHub → Actions**.

---

## 8. Verificar

1. `https://sistema-traction-api.azurewebsites.net/scalar/v1` → documentação da API
2. `https://sistema-traction-NOME.azurestaticapps.net` → sistema funcionando

---

## Custos mensais estimados

| Recurso | Plano | Custo |
|---|---|---|
| Azure App Service | B1 Basic | ~R$70/mês |
| Azure SQL Database | Basic 5 DTU | ~R$25/mês |
| Azure Static Web Apps | Free | R$0 |
| **Total** | | **~R$95/mês** |

---

## Domínio personalizado (opcional)

Após o sistema estar no ar, é possível configurar `sistema.minhaempresa.com.br`:
- App Service → Custom domains
- Static Web App → Custom domains

Sem custo adicional do Azure (só o custo do domínio em si).
