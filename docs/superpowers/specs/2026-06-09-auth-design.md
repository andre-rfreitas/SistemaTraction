# Design: Autenticação e Tela de Login

**Data:** 2026-06-09  
**Escopo:** Adicionar autenticação ao sistema — tela de login, proteção de todas as rotas da API e sessão segura via cookie HttpOnly.

---

## Contexto

O sistema está hospedado na internet (Azure Container Apps + Static Web Apps) e atualmente não tem nenhuma autenticação — qualquer pessoa com a URL acessa tudo. É um sistema de uso único (um único operador), então não há gestão de múltiplos usuários.

---

## Decisões de arquitetura

| Decisão | Escolha | Motivo |
|---------|---------|--------|
| Usuários | Senha única via variável de ambiente | Usuário único; sem necessidade de tabela de usuários |
| Armazenamento do token | Cookie HttpOnly + Secure + SameSite=Strict | JS não consegue ler o cookie; imune a XSS |
| Formato do token | JWT assinado com chave secreta | Stateless; fácil de validar no .NET |
| Expiração | 7 dias | Balanceia conveniência e segurança |
| Proteção contra força bruta | Rate limiting: 5 tentativas/min por IP | Bloqueia ataques automatizados |

---

## Backend

### Variáveis de ambiente (obrigatórias)

```
AUTH_PASSWORD_HASH   # hash BCrypt da senha (gerado uma vez com BCrypt.Net)
JWT_SECRET           # string aleatória longa (mínimo 32 chars) para assinar o JWT
```

Nunca armazenar a senha em texto plano. O hash é gerado offline e colocado na variável de ambiente.

### Novos arquivos

```
backend/src/
├── Application/
│   └── Auth/
│       └── Commands/
│           └── Login/
│               ├── LoginCommand.cs          # record com string Password
│               ├── LoginCommandHandler.cs   # verifica BCrypt, emite JWT
│               └── LoginCommandValidator.cs # valida que Password não é vazio
└── API/
    └── Controllers/
        └── AuthController.cs               # POST /api/auth/login, POST /api/auth/logout, GET /api/auth/me
```

### Mudanças em arquivos existentes

**`Program.cs`:**
- Adicionar `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` configurado para ler token do cookie
- Adicionar `builder.Services.AddRateLimiter` (política "login": 5 req/min por IP, aplicada só no endpoint de login)
- Adicionar `app.UseAuthentication()` **antes** de `app.UseAuthorization()`

**Todos os controllers existentes:**
- Adicionar atributo `[Authorize]` na classe

### Endpoints

#### `POST /api/auth/login`
- **Body:** `{ "password": "string" }`
- **Rate limit:** 5 tentativas por minuto por IP
- **Sucesso (200):** seta cookie `auth_token` com JWT; retorna `{ "ok": true }`
- **Falha (401):** retorna `{ "error": "Credenciais inválidas" }` — mensagem genérica, não revela se a senha existe

Cookie settings:
```
HttpOnly = true
Secure = true
SameSite = Strict
Expires = DateTime.UtcNow.AddDays(7)
Path = /
```

#### `POST /api/auth/logout`
- **Auth:** requer `[Authorize]`
- **Ação:** deleta o cookie `auth_token` (MaxAge = 0)
- **Retorno:** 200 OK

#### `GET /api/auth/me`
- **Auth:** requer `[Authorize]`
- **Retorno:** 200 `{ "authenticated": true }` se o cookie for válido; 401 se não

### Pacotes NuGet a adicionar

```
Microsoft.AspNetCore.Authentication.JwtBearer
BCrypt.Net-Next
```

---

## Frontend

### Novos arquivos

```
frontend/src/
├── features/auth/
│   ├── LoginPage.tsx                    # formulário de login (apenas campo de senha)
│   ├── schemas/loginSchema.ts           # schema Zod: { password: z.string().min(1) }
│   └── hooks/
│       ├── useLogin.ts                  # mutation POST /api/auth/login
│       └── useLogout.ts                 # mutation POST /api/auth/logout
└── lib/
    └── auth.tsx                         # AuthContext + AuthProvider + useAuth hook
```

### Mudanças em arquivos existentes

**`App.tsx`:**
```tsx
// Antes: renderiza AppShell diretamente
// Depois:
function App() {
  const { isAuthenticated, isLoading } = useAuth()
  if (isLoading) return <LoadingScreen />
  if (!isAuthenticated) return <LoginPage />
  return <AppShell .../>
}
```

**`main.tsx`:** envolve `<App />` com `<AuthProvider>`

**`lib/queryClient.ts`:** configurar `onError` global para redirecionar em respostas 401 (limpar estado de auth)

### Fluxo de verificação de sessão

```
App monta
  → AuthProvider chama GET /api/auth/me
    ✓ 200 → isAuthenticated = true → AppShell renderiza
    ✗ 401 → isAuthenticated = false → LoginPage renderiza
```

### Tela de login

- Campo único: senha (sem campo de usuário)
- Botão "Entrar"
- Estado de loading durante a requisição
- Mensagem de erro genérica em caso de 401: "Senha incorreta"
- Mesmo design visual do sistema (Tailwind + shadcn/ui)
- Sem "esqueci minha senha" (YAGNI para usuário único)

### Pacotes npm a adicionar

Nenhum — React Hook Form, Zod e TanStack Query já estão no projeto.

---

## Segurança — resumo das proteções

| Ameaça | Mitigação |
|--------|-----------|
| Força bruta | Rate limiting: 5 tentativas/min por IP |
| XSS roubando token | Cookie HttpOnly — JS não consegue ler |
| CSRF | SameSite=Strict — browser não envia cookie em requisições cross-site |
| Sniffing | Cookie Secure — só enviado via HTTPS |
| Senha exposta | BCrypt hash — senha real nunca armazenada |
| Enumeration | Mensagem de erro genérica no login |

---

## O que está fora do escopo

- Recuperação de senha
- 2FA (autenticação em dois fatores)
- Múltiplos usuários ou perfis de acesso
- Bloqueio de conta por tentativas excessivas (rate limiting por IP já cobre isso)
- Registro de usuários

---

## Ordem de implementação sugerida

1. Backend: configurar JWT + Authentication no `Program.cs`
2. Backend: criar `LoginCommand` + `LoginCommandHandler`
3. Backend: criar `AuthController`
4. Backend: adicionar `[Authorize]` em todos os controllers existentes
5. Frontend: criar `AuthContext` + `useAuth`
6. Frontend: criar `LoginPage` + hooks
7. Frontend: integrar `AuthProvider` no `main.tsx` e ajustar `App.tsx`
8. Deploy: adicionar `AUTH_PASSWORD_HASH` e `JWT_SECRET` nas variáveis de ambiente do Azure
