# Auth Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Adicionar autenticação completa ao sistema — tela de login no frontend e proteção de todas as rotas da API via JWT em cookie HttpOnly.

**Architecture:** Single-user auth: senha em variável de ambiente `AUTH_PASSWORD_HASH` (BCrypt), JWT assinado com `JWT_SECRET`, armazenado em cookie `HttpOnly; Secure; SameSite`. O `AuthController` lida com login/logout/me diretamente (sem MediatR — auth não é domínio de negócio). Todos os controllers existentes recebem `[Authorize]`. O frontend usa `AuthProvider` (React context) que verifica sessão via `GET /api/auth/me` ao carregar.

**Tech Stack:** .NET 9 + `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.5 + `BCrypt.Net-Next` 4.0.3 | React 18 + Axios + TanStack Query + React Hook Form + Zod

---

## Arquivos criados / modificados

### Backend
| Ação | Arquivo |
|------|---------|
| Modificar | `backend/src/API/SistemaTraction.API.csproj` |
| Modificar | `backend/src/API/Program.cs` |
| Criar | `backend/src/Application/Common/Interfaces/IAuthSettings.cs` |
| Criar | `backend/src/API/Controllers/AuthController.cs` |
| Modificar (13 arquivos) | `backend/src/API/Controllers/*.cs` — adicionar `[Authorize]` |

### Frontend
| Ação | Arquivo |
|------|---------|
| Modificar | `frontend/src/lib/api.ts` |
| Criar | `frontend/src/lib/auth.tsx` |
| Criar | `frontend/src/features/auth/schemas/loginSchema.ts` |
| Criar | `frontend/src/features/auth/hooks/useLogin.ts` |
| Criar | `frontend/src/features/auth/hooks/useLogout.ts` |
| Criar | `frontend/src/features/auth/LoginPage.tsx` |
| Modificar | `frontend/src/main.tsx` |
| Modificar | `frontend/src/App.tsx` |
| Modificar | `frontend/src/components/layout/Topbar.tsx` |

---

## Task 1: Adicionar pacotes NuGet e configurar autenticação JWT no backend

**Files:**
- Modify: `backend/src/API/SistemaTraction.API.csproj`
- Create: `backend/src/Application/Common/Interfaces/IAuthSettings.cs`
- Modify: `backend/src/API/Program.cs`

- [ ] **Step 1.1: Adicionar pacotes NuGet ao API.csproj**

Adicionar as duas referências de pacote dentro de `<ItemGroup>` existente em `backend/src/API/SistemaTraction.API.csproj`:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.5" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

O arquivo deve ficar:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Scalar.AspNetCore" Version="2.14.14" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Application\SistemaTraction.Application.csproj" />
    <ProjectReference Include="..\Infrastructure\SistemaTraction.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 1.2: Criar interface IAuthSettings**

Criar `backend/src/Application/Common/Interfaces/IAuthSettings.cs`:

```csharp
namespace SistemaTraction.Application.Common.Interfaces;

public interface IAuthSettings
{
    string PasswordHash { get; }
    string JwtSecret { get; }
}
```

- [ ] **Step 1.3: Atualizar Program.cs**

Substituir o conteúdo de `backend/src/API/Program.cs` pelo seguinte:

```csharp
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SistemaTraction.Application;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Infrastructure;
using SistemaTraction.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Auth settings — lidos uma vez ao iniciar, falha rápido se não configurados
builder.Services.AddSingleton<IAuthSettings>(new AuthSettings(
    builder.Configuration["AUTH_PASSWORD_HASH"]
        ?? throw new InvalidOperationException("AUTH_PASSWORD_HASH não configurado"),
    builder.Configuration["JWT_SECRET"]
        ?? throw new InvalidOperationException("JWT_SECRET não configurado")));

// JWT authentication — lê token do cookie auth_token
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET não configurado");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["auth_token"];
                return Task.CompletedTask;
            }
        };
    });

// Rate limiter — 5 tentativas de login por minuto por IP
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration["AllowedCorsOrigins"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? ["http://localhost:5173"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("Migrations aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Não foi possível aplicar migrations automaticamente. " +
            "Verifique se o LocalDB está rodando: sqllocaldb start UNI1500");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Vary", "Origin");
    await next();
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// AuthSettings implementa IAuthSettings — usado para injeção de dependência
internal record AuthSettings(string PasswordHash, string JwtSecret) : IAuthSettings;
```

- [ ] **Step 1.4: Verificar que compila**

```bash
cd backend
dotnet build
```

Esperado: `Build succeeded.`

---

## Task 2: Criar AuthController

**Files:**
- Create: `backend/src/API/Controllers/AuthController.cs`

- [ ] **Step 2.1: Criar AuthController**

Criar `backend/src/API/Controllers/AuthController.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SistemaTraction.Application.Common.Interfaces;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthSettings authSettings, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!BCrypt.Net.BCrypt.Verify(request.Password, authSettings.PasswordHash))
            return Unauthorized(new { error = "Credenciais inválidas" });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.JwtSecret));
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Response.Cookies.Append("auth_token", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = env.IsDevelopment() ? SameSiteMode.Strict : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });

        return Ok(new { ok = true });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new { authenticated = true });
}

public record LoginRequest(string Password);
```

- [ ] **Step 2.2: Verificar build**

```bash
cd backend
dotnet build
```

Esperado: `Build succeeded.`

---

## Task 3: Adicionar [Authorize] nos controllers existentes

**Files:** (modificar todos os 13 controllers existentes)
- `backend/src/API/Controllers/AppConfigController.cs`
- `backend/src/API/Controllers/CuttingOrdersController.cs`
- `backend/src/API/Controllers/DtfModelsController.cs`
- `backend/src/API/Controllers/DtfStockController.cs`
- `backend/src/API/Controllers/FabricRollsController.cs`
- `backend/src/API/Controllers/FabricTypesController.cs`
- `backend/src/API/Controllers/FinancialController.cs`
- `backend/src/API/Controllers/SeparationListsController.cs`
- `backend/src/API/Controllers/ShirtStockController.cs`
- `backend/src/API/Controllers/SupplyDeductionController.cs`
- `backend/src/API/Controllers/SupplyOrderConfigsController.cs`
- `backend/src/API/Controllers/SupplyStockController.cs`
- `backend/src/API/Controllers/SupplyTypesController.cs`

- [ ] **Step 3.1: Adicionar `using Microsoft.AspNetCore.Authorization;` e `[Authorize]` em cada controller**

Em cada um dos 13 arquivos acima, adicionar:
1. `using Microsoft.AspNetCore.Authorization;` junto aos outros usings
2. Atributo `[Authorize]` na linha antes de `public class`

Exemplo para `AppConfigController.cs` — adicionar as duas linhas marcadas com `+`:

```csharp
// + using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ... outros usings ...

// + [Authorize]
[ApiController]
[Route("api/app-config")]
public class AppConfigController(IMediator mediator) : ControllerBase
```

Repetir o mesmo padrão para todos os 13 controllers.

- [ ] **Step 3.2: Verificar build final do backend**

```bash
cd backend
dotnet build
dotnet test
```

Esperado: `Build succeeded.` e todos os testes passando.

---

## Task 4: Atualizar api.ts e criar AuthContext no frontend

**Files:**
- Modify: `frontend/src/lib/api.ts`
- Create: `frontend/src/lib/auth.tsx`

- [ ] **Step 4.1: Atualizar api.ts — withCredentials + evento de 401**

Substituir o conteúdo de `frontend/src/lib/api.ts`:

```ts
import axios from 'axios'

const baseURL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

export const api = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:logout'))
    }
    const message = error.response?.data?.error ?? 'Erro inesperado.'
    return Promise.reject(new Error(message))
  }
)
```

- [ ] **Step 4.2: Criar lib/auth.tsx**

Criar `frontend/src/lib/auth.tsx`:

```tsx
import { createContext, useContext, useEffect, useState } from 'react'
import { api } from './api'

interface AuthState {
  isAuthenticated: boolean
  isLoading: boolean
  setAuthenticated: (value: boolean) => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const handleUnauthorized = () => setAuthenticated(false)
    window.addEventListener('auth:logout', handleUnauthorized)
    return () => window.removeEventListener('auth:logout', handleUnauthorized)
  }, [])

  useEffect(() => {
    api
      .get('/auth/me')
      .then(() => setAuthenticated(true))
      .catch(() => setAuthenticated(false))
      .finally(() => setIsLoading(false))
  }, [])

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, setAuthenticated }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider')
  return ctx
}
```

---

## Task 5: Criar feature de autenticação no frontend

**Files:**
- Create: `frontend/src/features/auth/schemas/loginSchema.ts`
- Create: `frontend/src/features/auth/hooks/useLogin.ts`
- Create: `frontend/src/features/auth/hooks/useLogout.ts`
- Create: `frontend/src/features/auth/LoginPage.tsx`

- [ ] **Step 5.1: Criar loginSchema.ts**

Criar `frontend/src/features/auth/schemas/loginSchema.ts`:

```ts
import { z } from 'zod'

export const loginSchema = z.object({
  password: z.string().min(1, 'Senha é obrigatória'),
})

export type LoginFormData = z.infer<typeof loginSchema>
```

- [ ] **Step 5.2: Criar useLogin.ts**

Criar `frontend/src/features/auth/hooks/useLogin.ts`:

```ts
import { useMutation } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { useAuth } from '@/lib/auth'

export function useLogin() {
  const { setAuthenticated } = useAuth()
  return useMutation({
    mutationFn: (password: string) => api.post('/auth/login', { password }),
    onSuccess: () => setAuthenticated(true),
  })
}
```

- [ ] **Step 5.3: Criar useLogout.ts**

Criar `frontend/src/features/auth/hooks/useLogout.ts`:

```ts
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import { useAuth } from '@/lib/auth'

export function useLogout() {
  const { setAuthenticated } = useAuth()
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => api.post('/auth/logout'),
    onSuccess: () => {
      queryClient.clear()
      setAuthenticated(false)
    },
  })
}
```

- [ ] **Step 5.4: Criar LoginPage.tsx**

Criar `frontend/src/features/auth/LoginPage.tsx`:

```tsx
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { loginSchema, type LoginFormData } from './schemas/loginSchema'
import { useLogin } from './hooks/useLogin'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

export function LoginPage() {
  const { mutate: login, isPending, error } = useLogin()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  return (
    <div className="flex h-screen items-center justify-center bg-background">
      <div className="w-full max-w-sm space-y-6 rounded-lg border border-border bg-card p-8 shadow-sm">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold tracking-tight">StockShirt</h1>
          <p className="text-sm text-muted-foreground">Digite sua senha para continuar</p>
        </div>
        <form onSubmit={handleSubmit(({ password }) => login(password))} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="password">Senha</Label>
            <Input id="password" type="password" autoFocus {...register('password')} />
            {errors.password && (
              <p className="text-sm text-destructive">{errors.password.message}</p>
            )}
          </div>
          {error && <p className="text-sm text-destructive">Senha incorreta</p>}
          <Button type="submit" disabled={isPending} className="w-full">
            {isPending ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
      </div>
    </div>
  )
}
```

---

## Task 6: Integrar AuthProvider e adicionar botão de logout

**Files:**
- Modify: `frontend/src/main.tsx`
- Modify: `frontend/src/App.tsx`
- Modify: `frontend/src/components/layout/Topbar.tsx`

- [ ] **Step 6.1: Atualizar main.tsx — adicionar AuthProvider**

Substituir o conteúdo de `frontend/src/main.tsx`:

```tsx
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { queryClient } from '@/lib/queryClient'
import { ThemeProvider } from './lib/theme'
import { AuthProvider } from './lib/auth'
import App from './App.tsx'
import './index.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <App />
          <ReactQueryDevtools initialIsOpen={false} />
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  </StrictMode>,
)
```

- [ ] **Step 6.2: Atualizar App.tsx — verificar autenticação antes de renderizar**

Substituir o conteúdo de `frontend/src/App.tsx`:

```tsx
import { useState } from 'react'
import { AppShell } from '@/components/layout/AppShell'
import { useAuth } from '@/lib/auth'
import { LoginPage } from '@/features/auth/LoginPage'
import type { TabId } from '@/components/layout/nav'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { FabricRollPage } from '@/features/fabric/rolls/FabricRollPage'
import { CuttingOrderPage } from '@/features/cutting/orders/CuttingOrderPage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'
import { DtfStockPage } from '@/features/stock/dtf/DtfStockPage'
import { SeparationListPage } from '@/features/separation/SeparationListPage'
import { FinancialPage } from '@/features/financial/FinancialPage'
import { SettingsPage } from '@/features/settings/SettingsPage'
import { ShirtStockPage } from '@/features/stock/shirts/ShirtStockPage'
import { SupplyStockPage } from '@/features/stock/supplies/SupplyStockPage'
import { SupplyTypePage } from '@/features/settings/supplies/SupplyTypePage'

function App() {
  const { isAuthenticated, isLoading } = useAuth()
  const [tab, setTab] = useState<TabId>('financial')

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-background">
        <div className="size-6 animate-spin rounded-full border-2 border-primary border-t-transparent" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <LoginPage />
  }

  const pages: Record<TabId, React.ReactNode> = {
    fabric: <FabricTypePage />,
    rolls: <FabricRollPage />,
    cutting: <CuttingOrderPage />,
    'dtf-models': <DtfModelPage />,
    'dtf-stock': <DtfStockPage />,
    'shirt-stock': <ShirtStockPage />,
    'supply-stock': <SupplyStockPage />,
    'supply-types': <SupplyTypePage />,
    separation: <SeparationListPage />,
    financial: <FinancialPage />,
    config: <SettingsPage onNavigate={setTab} />,
  }

  return (
    <AppShell active={tab} onSelect={setTab}>
      {pages[tab]}
    </AppShell>
  )
}

export default App
```

- [ ] **Step 6.3: Atualizar Topbar.tsx — adicionar botão de logout**

Substituir o conteúdo de `frontend/src/components/layout/Topbar.tsx`:

```tsx
import { LogOut, Menu, PanelLeft, PanelLeftClose } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from '@/components/ui/theme-toggle'
import { useLogout } from '@/features/auth/hooks/useLogout'

interface TopbarProps {
  title: string
  collapsed: boolean
  onToggleCollapse: () => void
  onOpenMobile: () => void
}

export function Topbar({ title, collapsed, onToggleCollapse, onOpenMobile }: TopbarProps) {
  const { mutate: logout, isPending } = useLogout()

  return (
    <header className="flex h-14 items-center gap-2 border-b border-border bg-background/80 px-4 backdrop-blur">
      <Button
        variant="ghost"
        size="icon"
        className="lg:hidden"
        onClick={onOpenMobile}
        aria-label="Abrir menu"
      >
        <Menu className="size-4" />
      </Button>
      <Button
        variant="ghost"
        size="icon"
        className="hidden lg:inline-flex"
        onClick={onToggleCollapse}
        aria-label={collapsed ? 'Expandir menu' : 'Recolher menu'}
      >
        {collapsed ? <PanelLeft className="size-4" /> : <PanelLeftClose className="size-4" />}
      </Button>
      <span className="text-sm font-medium text-foreground">{title}</span>
      <div className="ml-auto flex items-center gap-1">
        <ThemeToggle />
        <Button
          variant="ghost"
          size="icon"
          onClick={() => logout()}
          disabled={isPending}
          aria-label="Sair"
        >
          <LogOut className="size-4" />
        </Button>
      </div>
    </header>
  )
}
```

- [ ] **Step 6.4: Verificar typecheck e lint**

```bash
cd frontend
pnpm typecheck
pnpm lint
```

Esperado: sem erros de tipo ou lint.

---

## Task 7: Build final, testes e commit

- [ ] **Step 7.1: Build e testes do backend**

```bash
cd backend
dotnet build
dotnet test
```

Esperado: `Build succeeded.` e todos os testes passando.

- [ ] **Step 7.2: Build do frontend**

```bash
cd frontend
pnpm build
```

Esperado: build completo sem erros.

- [ ] **Step 7.3: Commit único de tudo**

```bash
git add backend/src/API/SistemaTraction.API.csproj \
        backend/src/API/Program.cs \
        backend/src/Application/Common/Interfaces/IAuthSettings.cs \
        backend/src/API/Controllers/AuthController.cs \
        backend/src/API/Controllers/AppConfigController.cs \
        backend/src/API/Controllers/CuttingOrdersController.cs \
        backend/src/API/Controllers/DtfModelsController.cs \
        backend/src/API/Controllers/DtfStockController.cs \
        backend/src/API/Controllers/FabricRollsController.cs \
        backend/src/API/Controllers/FabricTypesController.cs \
        backend/src/API/Controllers/FinancialController.cs \
        backend/src/API/Controllers/SeparationListsController.cs \
        backend/src/API/Controllers/ShirtStockController.cs \
        backend/src/API/Controllers/SupplyDeductionController.cs \
        backend/src/API/Controllers/SupplyOrderConfigsController.cs \
        backend/src/API/Controllers/SupplyStockController.cs \
        backend/src/API/Controllers/SupplyTypesController.cs \
        frontend/src/lib/api.ts \
        frontend/src/lib/auth.tsx \
        frontend/src/features/auth/ \
        frontend/src/main.tsx \
        frontend/src/App.tsx \
        frontend/src/components/layout/Topbar.tsx
git commit -m "feat: adicionar autenticação — tela de login, JWT em cookie HttpOnly, proteção de todas as rotas"
```

---

## Notas de deploy (Azure)

Após o deploy, adicionar as variáveis de ambiente no Azure Container App:

```bash
# Gerar hash da senha (rodar localmente uma vez):
dotnet script -e 'Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("SUA_SENHA_AQUI"));'
# OU usar online BCrypt generator com work factor 10+

# No Azure Portal ou CLI:
AUTH_PASSWORD_HASH=<hash_gerado_acima>
JWT_SECRET=<string_aleatoria_minimo_32_chars>
```

Sem essas variáveis configuradas, o backend falhará ao iniciar (comportamento intencional — fail fast).
