# Setup Azure — SistemaTraction

Guia passo a passo para criar os recursos no Azure e configurar o deploy automático.

## Arquitetura

| Componente | Serviço | Custo |
|---|---|---|
| Frontend | Azure Static Web Apps | Grátis |
| Backend .NET | Azure Container Apps | ~R$0-15/mês |
| Banco | Azure SQL Database Basic | ~R$25/mês |
| **Total** | | **~R$25-40/mês** |

---

## Pré-requisitos

- Conta Azure com assinatura ativa (portal.azure.com)
- Azure CLI instalado e logado: `az login`
- GitHub CLI instalado e logado: `gh auth login`
- Repositório no GitHub: github.com/andre-rfreitas/SistemaTraction

---

## 1. Criar Resource Group

```bash
az group create --name rg-sistema-traction --location brazilsouth
```

---

## 2. Criar Azure SQL Database

No portal.azure.com → buscar "SQL databases" → Criar:

| Campo | Valor |
|---|---|
| Resource group | `rg-sistema-traction` |
| Database name | `sistema-traction-db` |
| Server | Criar novo: `sistema-traction-sql`, SQL authentication, usuário e senha fortes |
| Compute + storage | Basic (5 DTUs) |
| Backup redundancy | Locally redundant |
| Networking | Ponto de extremidade público, Allow Azure services = Yes |

Após criar, copiar a connection string ADO.NET (autenticação SQL) em:
Portal → SQL Database → Cadeias de conexão → ADO.NET

---

## 3. Tornar a imagem Docker pública no GitHub

O Container Apps vai baixar a imagem do GitHub Container Registry (ghcr.io). Para isso a imagem precisa ser pública:

1. Acesse: github.com/andre-rfreitas?tab=packages
2. Após o primeiro deploy, o pacote `sistema-traction-api` aparecerá aqui
3. Clique nele → Package settings → Change visibility → **Public**

> A imagem contém apenas o binário compilado, não o código-fonte.

---

## 4. Criar Azure Container Apps

```bash
# Instalar extensão (só na primeira vez)
az extension add --name containerapp --upgrade

# Registrar providers
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights

# Criar environment
az containerapp env create \
  --name sistema-traction-env \
  --resource-group rg-sistema-traction \
  --location brazilsouth

# Criar o container app (imagem placeholder — o GitHub Actions vai atualizar)
az containerapp create \
  --name sistema-traction-api \
  --resource-group rg-sistema-traction \
  --environment sistema-traction-env \
  --image mcr.microsoft.com/dotnet/samples:aspnetapp \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 2 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "ConnectionStrings__DefaultConnection=SUA_CONNECTION_STRING_AQUI" \
    "AllowedCorsOrigins=https://NOME-DO-APP.azurestaticapps.net"
```

> Substitua `SUA_CONNECTION_STRING_AQUI` pela string do passo 2 (com a senha real).
> O `AllowedCorsOrigins` será atualizado após criar o Static Web App (passo 6).

Pegar a URL do backend:
```bash
az containerapp show \
  --name sistema-traction-api \
  --resource-group rg-sistema-traction \
  --query properties.configuration.ingress.fqdn \
  --output tsv
```

Guarde essa URL (ex: `sistema-traction-api.azurecontainerapps.io`).

---

## 5. Criar credenciais para o GitHub Actions

```bash
az ad sp create-for-rbac \
  --name "sistema-traction-deploy" \
  --role contributor \
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/rg-sistema-traction \
  --sdk-auth
```

Copie o JSON completo retornado. No GitHub:
- Settings → Secrets and variables → Actions → New repository secret
- Nome: `AZURE_CREDENTIALS`
- Valor: JSON completo copiado

---

## 6. Criar Azure Static Web Apps (frontend)

No portal.azure.com → buscar "Static Web Apps" → Criar:

| Campo | Valor |
|---|---|
| Resource group | `rg-sistema-traction` |
| Name | `sistema-traction` |
| Plan type | **Free** |
| Region | East US 2 |
| Source | GitHub → autorizar → repo SistemaTraction, branch main |
| App location | `/frontend` |
| Output location | `dist` |
| API location | (deixar vazio) |

Após criar:
- **Deletar** o workflow gerado automaticamente pelo Azure no repositório
- Copiar a URL gerada (ex: `https://sistema-traction-abc123.azurestaticapps.net`)
- Pegar o token: Portal → Static Web App → Manage deployment token

No GitHub, adicionar secrets:
| Secret | Valor |
|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | token do Static Web App |
| `VITE_API_URL` | `https://sistema-traction-api.REGIÃO.azurecontainerapps.io` (URL do passo 4) |

---

## 7. Atualizar CORS com a URL do Static Web App

```bash
az containerapp update \
  --name sistema-traction-api \
  --resource-group rg-sistema-traction \
  --set-env-vars "AllowedCorsOrigins=https://sistema-traction-abc123.azurestaticapps.net"
```

Atualizar também `backend/src/API/appsettings.Production.json`:
```json
{
  "AllowedCorsOrigins": "https://sistema-traction-abc123.azurestaticapps.net"
}
```

---

## 8. Fazer o primeiro deploy

```bash
git add .
git commit -m "config: atualizar CORS para URL de produção"
git push origin main
```

Acompanhar em: GitHub → Actions → ver os dois workflows rodando.

---

## 9. Verificar

1. `https://sistema-traction-api.REGIÃO.azurecontainerapps.io/scalar/v1` → documentação da API
2. `https://sistema-traction-abc123.azurestaticapps.net` → sistema funcionando

---

## Secrets do GitHub — resumo final

| Secret | Como obter |
|---|---|
| `AZURE_CREDENTIALS` | Saída do comando `az ad sp create-for-rbac` (passo 5) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Static Web App → Manage deployment token (passo 6) |
| `VITE_API_URL` | URL do Container App sem `/` no final (passo 4) |
