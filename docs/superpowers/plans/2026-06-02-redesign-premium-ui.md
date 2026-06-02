# Redesign Premium UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reconstruir a camada visual do frontend (tokens light/dark, app shell com sidebar, primitivos premium) e aplicá-la na tela Financeiro como vitrine, sem tocar em regra de negócio, APIs, hooks ou schemas.

**Architecture:** Tokens semânticos em CSS variables (`index.css`) mapeados para utilitários Tailwind v4 via `@theme inline`, com bloco `.dark`. Primitivos em `components/ui/*` consomem só tokens (`bg-background`, `text-foreground`, `border-border`...). Um `AppShell` (sidebar agrupada + topbar) substitui as abas no topo, mantendo o `useState<Tab>` de `App.tsx` (sem router novo). A tela Financeiro é reconstruída sobre os novos primitivos.

**Tech Stack:** React 19, TypeScript, Tailwind CSS v4 (`@tailwindcss/vite`), Radix UI, class-variance-authority, lucide-react, `@fontsource-variable/inter`, Vitest + Testing Library, pnpm.

---

## Convenções deste plano

- Diretório de trabalho dos comandos: `frontend/`.
- Lint: `pnpm lint`. Typecheck: `pnpm exec tsc --noEmit`. Testes: `pnpm test` (script adicionado na Task 1).
- Commits em português, sem comentários desnecessários no código (CLAUDE.md).
- Não alterar: hooks (`use*.ts`), `schemas/`, `types.ts`, `lib/api.ts`, `lib/queryClient.ts`.

---

## File Structure

**Criar:**
- `frontend/src/lib/theme.tsx` — provider de tema + hook `useTheme`
- `frontend/src/components/ui/theme-toggle.tsx` — botão sol/lua
- `frontend/src/components/ui/card.tsx`
- `frontend/src/components/ui/table.tsx`
- `frontend/src/components/ui/page-header.tsx`
- `frontend/src/components/ui/empty-state.tsx`
- `frontend/src/components/ui/skeleton.tsx`
- `frontend/src/components/ui/stat-card.tsx`
- `frontend/src/components/layout/Sidebar.tsx`
- `frontend/src/components/layout/Topbar.tsx`
- `frontend/src/components/layout/AppShell.tsx`
- `frontend/src/components/layout/nav.ts` — config de navegação (grupos/itens/ícones)
- Testes: `*.test.tsx` ao lado de cada primitivo com comportamento testável.

**Modificar:**
- `frontend/package.json` — script `test`, dep `@fontsource-variable/inter`
- `frontend/src/index.css` — tokens + `@theme inline`
- `frontend/src/main.tsx` — import Inter + `ThemeProvider`
- `frontend/src/App.tsx` — usar `AppShell`
- `frontend/src/components/ui/button.tsx` — tokens + estado loading
- `frontend/src/components/ui/input.tsx` — tokens + estado erro
- `frontend/src/components/ui/badge.tsx` — variantes semânticas
- `frontend/src/components/ui/dialog.tsx` — tokens + `DialogFooter`
- `frontend/src/features/financial/FinancialPage.tsx`
- `frontend/src/features/financial/components/SummaryCards.tsx`
- `frontend/src/features/financial/components/EntriesTable.tsx`

**Remover:**
- `frontend/src/App.css` (boilerplate morto do Vite)

---

## Task 1: Setup — script de teste, fonte Inter, tokens de design e remoção de boilerplate

**Files:**
- Modify: `frontend/package.json`
- Modify: `frontend/src/index.css`
- Modify: `frontend/src/main.tsx`
- Delete: `frontend/src/App.css`

- [ ] **Step 1: Adicionar dependência Inter e script de teste**

Run:
```bash
cd frontend && pnpm add @fontsource-variable/inter && pnpm pkg set scripts.test="vitest run" scripts.test:watch="vitest"
```
Expected: `@fontsource-variable/inter` aparece em `dependencies`; scripts `test`/`test:watch` no `package.json`.

- [ ] **Step 2: Substituir `index.css` pelos tokens completos**

Substituir todo o conteúdo de `frontend/src/index.css` por:

```css
@import "tailwindcss";
@import "@fontsource-variable/inter";

@custom-variant dark (&:where(.dark, .dark *));

:root {
  --background: #ffffff;
  --foreground: #0f172a;
  --card: #ffffff;
  --card-foreground: #0f172a;
  --popover: #ffffff;
  --popover-foreground: #0f172a;
  --muted: #f1f5f9;
  --muted-foreground: #64748b;
  --border: #e2e8f0;
  --input: #e2e8f0;
  --ring: #4f46e5;
  --primary: #0f172a;
  --primary-foreground: #f8fafc;
  --secondary: #f1f5f9;
  --secondary-foreground: #0f172a;
  --accent: #4f46e5;
  --accent-foreground: #ffffff;
  --destructive: #dc2626;
  --destructive-foreground: #f8fafc;
  --success: #059669;
  --warning: #d97706;
  --danger: #dc2626;
  --info: #2563eb;
  --radius: 0.625rem;
  --shadow-sm: 0 1px 2px 0 rgb(15 23 42 / 0.04), 0 1px 2px -1px rgb(15 23 42 / 0.06);
  --shadow-md: 0 2px 4px -1px rgb(15 23 42 / 0.06), 0 4px 8px -2px rgb(15 23 42 / 0.08);
  --shadow-lg: 0 8px 16px -4px rgb(15 23 42 / 0.08), 0 12px 24px -6px rgb(15 23 42 / 0.10);
}

.dark {
  --background: #0b1120;
  --foreground: #e6edf6;
  --card: #111a2c;
  --card-foreground: #e6edf6;
  --popover: #111a2c;
  --popover-foreground: #e6edf6;
  --muted: #1e293b;
  --muted-foreground: #94a3b8;
  --border: #1e293b;
  --input: #243044;
  --ring: #6366f1;
  --primary: #f8fafc;
  --primary-foreground: #0b1120;
  --secondary: #1e293b;
  --secondary-foreground: #e6edf6;
  --accent: #6366f1;
  --accent-foreground: #ffffff;
  --destructive: #ef4444;
  --destructive-foreground: #f8fafc;
  --success: #10b981;
  --warning: #f59e0b;
  --danger: #ef4444;
  --info: #3b82f6;
  --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.30);
  --shadow-md: 0 2px 6px -1px rgb(0 0 0 / 0.40);
  --shadow-lg: 0 10px 24px -6px rgb(0 0 0 / 0.50);
}

@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-card: var(--card);
  --color-card-foreground: var(--card-foreground);
  --color-popover: var(--popover);
  --color-popover-foreground: var(--popover-foreground);
  --color-muted: var(--muted);
  --color-muted-foreground: var(--muted-foreground);
  --color-border: var(--border);
  --color-input: var(--input);
  --color-ring: var(--ring);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  --color-secondary: var(--secondary);
  --color-secondary-foreground: var(--secondary-foreground);
  --color-accent: var(--accent);
  --color-accent-foreground: var(--accent-foreground);
  --color-destructive: var(--destructive);
  --color-destructive-foreground: var(--destructive-foreground);
  --color-success: var(--success);
  --color-warning: var(--warning);
  --color-danger: var(--danger);
  --color-info: var(--info);
  --radius-sm: calc(var(--radius) - 4px);
  --radius-md: calc(var(--radius) - 2px);
  --radius-lg: var(--radius);
  --shadow-sm: var(--shadow-sm);
  --shadow-md: var(--shadow-md);
  --shadow-lg: var(--shadow-lg);
  --font-sans: "Inter Variable", ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif;
}

@layer base {
  * {
    border-color: var(--border);
  }
  body {
    background-color: var(--background);
    color: var(--foreground);
    font-family: var(--font-sans);
    -webkit-font-smoothing: antialiased;
  }
  @media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
      animation-duration: 0.01ms !important;
      transition-duration: 0.01ms !important;
    }
  }
}
```

- [ ] **Step 3: Atualizar `main.tsx` (sem ThemeProvider ainda — adicionado na Task 2)**

Garantir que `main.tsx` importe `./index.css` (já deve importar) e **remover** qualquer import de `./App.css`. Conferir conteúdo atual e, se houver `import './App.css'`, apagar a linha.

- [ ] **Step 4: Remover o boilerplate morto**

Run:
```bash
cd frontend && git rm src/App.css
```
Expected: arquivo removido. Confirmar com `grep -rn "App.css" src` → sem resultados.

- [ ] **Step 5: Verificar build**

Run: `cd frontend && pnpm build`
Expected: build conclui sem erro (tokens compilam; classes como `bg-background` ficam disponíveis).

- [ ] **Step 6: Commit**

```bash
git add frontend/package.json frontend/pnpm-lock.yaml frontend/src/index.css frontend/src/main.tsx
git commit -m "feat: design tokens light/dark, fonte Inter e script de teste"
```

---

## Task 2: Provider de tema + ThemeToggle

**Files:**
- Create: `frontend/src/lib/theme.tsx`
- Create: `frontend/src/components/ui/theme-toggle.tsx`
- Create: `frontend/src/lib/theme.test.tsx`
- Modify: `frontend/src/main.tsx`

- [ ] **Step 1: Escrever o teste de comportamento**

Create `frontend/src/lib/theme.test.tsx`:

```tsx
import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ThemeProvider } from './theme'
import { ThemeToggle } from '@/components/ui/theme-toggle'

beforeEach(() => {
  localStorage.clear()
  document.documentElement.classList.remove('dark')
})

test('alterna a classe dark no html e persiste no localStorage', async () => {
  const user = userEvent.setup()
  render(
    <ThemeProvider>
      <ThemeToggle />
    </ThemeProvider>,
  )
  expect(document.documentElement.classList.contains('dark')).toBe(false)
  await act(async () => {
    await user.click(screen.getByRole('button', { name: /tema/i }))
  })
  expect(document.documentElement.classList.contains('dark')).toBe(true)
  expect(localStorage.getItem('theme')).toBe('dark')
})
```

- [ ] **Step 2: Rodar o teste e ver falhar**

Run: `cd frontend && pnpm test src/lib/theme.test.tsx`
Expected: FAIL (módulos `./theme` e `theme-toggle` não existem).

- [ ] **Step 3: Implementar o provider**

Create `frontend/src/lib/theme.tsx`:

```tsx
import { createContext, useContext, useEffect, useState } from 'react'

type Theme = 'light' | 'dark'
type ThemeContextValue = { theme: Theme; toggleTheme: () => void }

const ThemeContext = createContext<ThemeContextValue | null>(null)

function getInitialTheme(): Theme {
  const stored = localStorage.getItem('theme')
  if (stored === 'light' || stored === 'dark') return stored
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>(getInitialTheme)

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark')
    localStorage.setItem('theme', theme)
  }, [theme])

  const toggleTheme = () => setTheme((t) => (t === 'dark' ? 'light' : 'dark'))

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  )
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme deve ser usado dentro de ThemeProvider')
  return ctx
}
```

- [ ] **Step 4: Implementar o ThemeToggle**

Create `frontend/src/components/ui/theme-toggle.tsx`:

```tsx
import { Moon, Sun } from 'lucide-react'
import { useTheme } from '@/lib/theme'
import { Button } from './button'

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme()
  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={toggleTheme}
      aria-label={theme === 'dark' ? 'Mudar para tema claro' : 'Mudar para tema escuro'}
    >
      {theme === 'dark' ? <Sun className="size-4" /> : <Moon className="size-4" />}
    </Button>
  )
}
```

- [ ] **Step 5: Embrulhar o app no ThemeProvider**

Modificar `frontend/src/main.tsx` para envolver a árvore com `<ThemeProvider>` (dentro do `QueryClientProvider`, em volta de `<App />`). Importar `import { ThemeProvider } from './lib/theme'`.

- [ ] **Step 6: Rodar o teste e ver passar**

Run: `cd frontend && pnpm test src/lib/theme.test.tsx`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add frontend/src/lib/theme.tsx frontend/src/lib/theme.test.tsx frontend/src/components/ui/theme-toggle.tsx frontend/src/main.tsx
git commit -m "feat: provider de tema com toggle light/dark persistido"
```

---

## Task 3: Refatorar Button (tokens + estado loading)

**Files:**
- Modify: `frontend/src/components/ui/button.tsx`
- Test: `frontend/src/components/ui/button.test.tsx`

- [ ] **Step 1: Escrever o teste**

Create `frontend/src/components/ui/button.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import { Button } from './button'

test('em loading fica desabilitado e mostra spinner', () => {
  render(<Button isLoading>Salvar</Button>)
  const btn = screen.getByRole('button', { name: /salvar/i })
  expect(btn).toBeDisabled()
  expect(btn.querySelector('svg')).toBeInTheDocument()
})
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `cd frontend && pnpm test src/components/ui/button.test.tsx`
Expected: FAIL (prop `isLoading` não existe).

- [ ] **Step 3: Reescrever o Button**

Substituir `frontend/src/components/ui/button.tsx` por:

```tsx
import * as React from 'react'
import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        primary: 'bg-primary text-primary-foreground shadow-sm hover:bg-primary/90',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        outline: 'border border-border bg-background shadow-sm hover:bg-muted',
        ghost: 'hover:bg-muted hover:text-foreground',
        destructive: 'bg-destructive text-destructive-foreground shadow-sm hover:bg-destructive/90',
        link: 'text-accent underline-offset-4 hover:underline',
      },
      size: {
        default: 'h-9 px-4 py-2',
        sm: 'h-8 rounded-md px-3 text-xs',
        lg: 'h-10 rounded-md px-6',
        icon: 'size-9',
      },
    },
    defaultVariants: { variant: 'primary', size: 'default' },
  },
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
  isLoading?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, isLoading = false, disabled, children, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button'
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        disabled={disabled || isLoading}
        {...props}
      >
        {isLoading && <Loader2 className="size-4 animate-spin" />}
        {children}
      </Comp>
    )
  },
)
Button.displayName = 'Button'

export { Button, buttonVariants }
```

> Nota de migração: a variante padrão passou de `default` para `primary`. Há um uso de `variant="default"` em `EntriesTable.tsx` (Badge, não Button) — não afeta. Buscar usos de `<Button variant="default"` com `grep -rn 'variant="default"' src` e trocar para `primary` se houver Button.

- [ ] **Step 4: Ajustar usos quebrados de Button**

Run: `cd frontend && grep -rn 'Button[^>]*variant="default"' src || echo "nenhum"`
Para cada ocorrência em Button, trocar `variant="default"` por `variant="primary"`. (Botões sem `variant` continuam funcionando — default agora é primary.)

- [ ] **Step 5: Rodar teste + typecheck**

Run: `cd frontend && pnpm test src/components/ui/button.test.tsx && pnpm exec tsc --noEmit`
Expected: teste PASS, typecheck sem erros.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/components/ui/button.tsx frontend/src/components/ui/button.test.tsx
git commit -m "feat: Button com tokens e estado de loading"
```

---

## Task 4: Refatorar Input (tokens + estado de erro)

**Files:**
- Modify: `frontend/src/components/ui/input.tsx`

- [ ] **Step 1: Reescrever o Input**

Substituir `frontend/src/components/ui/input.tsx` por:

```tsx
import * as React from 'react'
import { cn } from '@/lib/utils'

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: boolean
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, error, ...props }, ref) => {
    return (
      <input
        type={type}
        ref={ref}
        aria-invalid={error || undefined}
        className={cn(
          'flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors',
          'placeholder:text-muted-foreground',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 focus-visible:ring-offset-background',
          'disabled:cursor-not-allowed disabled:opacity-50',
          'aria-[invalid=true]:border-destructive aria-[invalid=true]:focus-visible:ring-destructive',
          className,
        )}
        {...props}
      />
    )
  },
)
Input.displayName = 'Input'

export { Input }
```

- [ ] **Step 2: Typecheck**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros (a prop `error` é opcional; usos atuais não quebram).

- [ ] **Step 3: Commit**

```bash
git add frontend/src/components/ui/input.tsx
git commit -m "feat: Input com tokens e estado de erro acessível"
```

---

## Task 5: Refatorar Badge (variantes semânticas)

**Files:**
- Modify: `frontend/src/components/ui/badge.tsx`
- Test: `frontend/src/components/ui/badge.test.tsx`

- [ ] **Step 1: Escrever o teste**

Create `frontend/src/components/ui/badge.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import { Badge } from './badge'

test('aplica classe da variante success', () => {
  render(<Badge variant="success">Receita</Badge>)
  expect(screen.getByText('Receita').className).toMatch(/success/)
})
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `cd frontend && pnpm test src/components/ui/badge.test.tsx`
Expected: FAIL (variante `success` não existe).

- [ ] **Step 3: Reescrever o Badge**

Substituir `frontend/src/components/ui/badge.tsx` por:

```tsx
import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const badgeVariants = cva(
  'inline-flex items-center rounded-md border px-2 py-0.5 text-xs font-medium transition-colors',
  {
    variants: {
      variant: {
        neutral: 'border-transparent bg-muted text-muted-foreground',
        primary: 'border-transparent bg-primary text-primary-foreground',
        success: 'border-success/20 bg-success/10 text-success',
        warning: 'border-warning/20 bg-warning/10 text-warning',
        danger: 'border-danger/20 bg-danger/10 text-danger',
        info: 'border-info/20 bg-info/10 text-info',
        outline: 'border-border text-foreground',
      },
    },
    defaultVariants: { variant: 'neutral' },
  },
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLSpanElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />
}

export { Badge, badgeVariants }
```

> Nota: `EntriesTable.tsx` usa hoje `variant="default"` e `variant="secondary"` no Badge. Serão atualizados na Task 14 (`default`→`success`/`info`, `secondary`→`neutral`).

- [ ] **Step 4: Rodar teste**

Run: `cd frontend && pnpm test src/components/ui/badge.test.tsx`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/components/ui/badge.tsx frontend/src/components/ui/badge.test.tsx
git commit -m "feat: Badge com variantes semanticas"
```

---

## Task 6: Refatorar Dialog (tokens + DialogFooter)

**Files:**
- Modify: `frontend/src/components/ui/dialog.tsx`

- [ ] **Step 1: Ler o Dialog atual**

Run: `cd frontend && cat src/components/ui/dialog.tsx`
Localizar as classes hardcoded (`bg-white`, `border-neutral-*`, `bg-black/...` no overlay) e a ausência de `DialogFooter`.

- [ ] **Step 2: Atualizar classes para tokens e adicionar DialogFooter**

No `dialog.tsx`:
- Overlay: usar `bg-foreground/40 backdrop-blur-sm` (em vez de `bg-black/...`).
- Content: trocar `bg-white border-neutral-200` por `bg-card text-card-foreground border border-border rounded-lg shadow-lg`.
- `DialogTitle`: `text-base font-semibold text-foreground`.
- Acrescentar e exportar:

```tsx
function DialogFooter({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('flex flex-col-reverse gap-2 sm:flex-row sm:justify-end', className)}
      {...props}
    />
  )
}
```
Adicionar `DialogFooter` ao bloco de `export { ... }`.

- [ ] **Step 3: Typecheck**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/components/ui/dialog.tsx
git commit -m "feat: Dialog com tokens e DialogFooter"
```

---

## Task 7: Primitivo Card

**Files:**
- Create: `frontend/src/components/ui/card.tsx`

- [ ] **Step 1: Implementar o Card**

Create `frontend/src/components/ui/card.tsx`:

```tsx
import * as React from 'react'
import { cn } from '@/lib/utils'

function Card({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('rounded-lg border border-border bg-card text-card-foreground shadow-sm', className)}
      {...props}
    />
  )
}

function CardHeader({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex flex-col gap-1 p-5', className)} {...props} />
}

function CardTitle({ className, ...props }: React.HTMLAttributes<HTMLHeadingElement>) {
  return <h3 className={cn('text-base font-semibold leading-none', className)} {...props} />
}

function CardDescription({ className, ...props }: React.HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('text-sm text-muted-foreground', className)} {...props} />
}

function CardContent({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-5 pt-0', className)} {...props} />
}

function CardFooter({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex items-center p-5 pt-0', className)} {...props} />
}

export { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter }
```

- [ ] **Step 2: Typecheck + commit**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros.
```bash
git add frontend/src/components/ui/card.tsx
git commit -m "feat: primitivo Card"
```

---

## Task 8: Primitivo Table

**Files:**
- Create: `frontend/src/components/ui/table.tsx`

- [ ] **Step 1: Implementar o Table**

Create `frontend/src/components/ui/table.tsx`:

```tsx
import * as React from 'react'
import { cn } from '@/lib/utils'

function Table({ className, ...props }: React.HTMLAttributes<HTMLTableElement>) {
  return (
    <div className="w-full overflow-x-auto rounded-lg border border-border bg-card">
      <table className={cn('w-full text-sm', className)} {...props} />
    </div>
  )
}

function TableHeader({ className, ...props }: React.HTMLAttributes<HTMLTableSectionElement>) {
  return <thead className={cn('[&_tr]:border-b [&_tr]:border-border', className)} {...props} />
}

function TableBody({ className, ...props }: React.HTMLAttributes<HTMLTableSectionElement>) {
  return <tbody className={cn('[&_tr:last-child]:border-0', className)} {...props} />
}

function TableRow({ className, ...props }: React.HTMLAttributes<HTMLTableRowElement>) {
  return (
    <tr
      className={cn('border-b border-border transition-colors hover:bg-muted/50', className)}
      {...props}
    />
  )
}

function TableHead({ className, ...props }: React.ThHTMLAttributes<HTMLTableCellElement>) {
  return (
    <th
      className={cn(
        'h-10 px-4 text-left align-middle text-xs font-medium uppercase tracking-wide text-muted-foreground',
        className,
      )}
      {...props}
    />
  )
}

function TableCell({ className, ...props }: React.TdHTMLAttributes<HTMLTableCellElement>) {
  return <td className={cn('px-4 py-2.5 align-middle', className)} {...props} />
}

export { Table, TableHeader, TableBody, TableRow, TableHead, TableCell }
```

- [ ] **Step 2: Typecheck + commit**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros.
```bash
git add frontend/src/components/ui/table.tsx
git commit -m "feat: primitivo Table"
```

---

## Task 9: Primitivos PageHeader, EmptyState, Skeleton

**Files:**
- Create: `frontend/src/components/ui/page-header.tsx`
- Create: `frontend/src/components/ui/empty-state.tsx`
- Create: `frontend/src/components/ui/skeleton.tsx`
- Test: `frontend/src/components/ui/empty-state.test.tsx`

- [ ] **Step 1: Implementar PageHeader**

Create `frontend/src/components/ui/page-header.tsx`:

```tsx
import * as React from 'react'
import { cn } from '@/lib/utils'

interface PageHeaderProps {
  title: string
  description?: string
  actions?: React.ReactNode
  className?: string
}

export function PageHeader({ title, description, actions, className }: PageHeaderProps) {
  return (
    <div className={cn('flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between', className)}>
      <div className="space-y-1">
        <h1 className="text-xl font-semibold tracking-tight text-foreground">{title}</h1>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      {actions && <div className="flex shrink-0 items-center gap-2">{actions}</div>}
    </div>
  )
}
```

- [ ] **Step 2: Implementar Skeleton**

Create `frontend/src/components/ui/skeleton.tsx`:

```tsx
import { cn } from '@/lib/utils'

export function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('animate-pulse rounded-md bg-muted', className)} {...props} />
}
```

- [ ] **Step 3: Escrever teste do EmptyState**

Create `frontend/src/components/ui/empty-state.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import { Inbox } from 'lucide-react'
import { EmptyState } from './empty-state'

test('renderiza titulo, descricao e acao', () => {
  render(
    <EmptyState
      icon={Inbox}
      title="Sem lançamentos"
      description="Nada por aqui ainda."
      action={<button>Adicionar</button>}
    />,
  )
  expect(screen.getByText('Sem lançamentos')).toBeInTheDocument()
  expect(screen.getByText('Nada por aqui ainda.')).toBeInTheDocument()
  expect(screen.getByRole('button', { name: 'Adicionar' })).toBeInTheDocument()
})
```

- [ ] **Step 4: Rodar e ver falhar**

Run: `cd frontend && pnpm test src/components/ui/empty-state.test.tsx`
Expected: FAIL (módulo não existe).

- [ ] **Step 5: Implementar EmptyState**

Create `frontend/src/components/ui/empty-state.tsx`:

```tsx
import * as React from 'react'
import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description?: string
  action?: React.ReactNode
  className?: string
}

export function EmptyState({ icon: Icon, title, description, action, className }: EmptyStateProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center gap-3 rounded-lg border border-dashed border-border bg-card px-6 py-12 text-center',
        className,
      )}
    >
      {Icon && (
        <div className="flex size-10 items-center justify-center rounded-full bg-muted">
          <Icon className="size-5 text-muted-foreground" />
        </div>
      )}
      <div className="space-y-1">
        <p className="text-sm font-medium text-foreground">{title}</p>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      {action}
    </div>
  )
}
```

> Nota: `lucide-react@1.x` exporta `LucideIcon` como tipo. Se o typecheck reclamar do import de tipo, usar `import { type LucideIcon } from 'lucide-react'`.

- [ ] **Step 6: Rodar teste + typecheck**

Run: `cd frontend && pnpm test src/components/ui/empty-state.test.tsx && pnpm exec tsc --noEmit`
Expected: PASS e sem erros de tipo.

- [ ] **Step 7: Commit**

```bash
git add frontend/src/components/ui/page-header.tsx frontend/src/components/ui/skeleton.tsx frontend/src/components/ui/empty-state.tsx frontend/src/components/ui/empty-state.test.tsx
git commit -m "feat: primitivos PageHeader, Skeleton e EmptyState"
```

---

## Task 10: Primitivo StatCard (KPI)

**Files:**
- Create: `frontend/src/components/ui/stat-card.tsx`

- [ ] **Step 1: Implementar StatCard**

Create `frontend/src/components/ui/stat-card.tsx`:

```tsx
import * as React from 'react'
import { cn } from '@/lib/utils'

type Tone = 'default' | 'success' | 'danger'

interface StatCardProps {
  label: string
  value: React.ReactNode
  hint?: string
  tone?: Tone
  className?: string
}

const toneClass: Record<Tone, string> = {
  default: 'text-foreground',
  success: 'text-success',
  danger: 'text-danger',
}

export function StatCard({ label, value, hint, tone = 'default', className }: StatCardProps) {
  return (
    <div className={cn('rounded-lg border border-border bg-card p-4 shadow-sm', className)}>
      <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className={cn('mt-1.5 text-2xl font-semibold tabular-nums', toneClass[tone])}>{value}</p>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </div>
  )
}
```

- [ ] **Step 2: Typecheck + commit**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros.
```bash
git add frontend/src/components/ui/stat-card.tsx
git commit -m "feat: primitivo StatCard para KPIs"
```

---

## Task 11: Config de navegação + Sidebar + Topbar + AppShell

**Files:**
- Create: `frontend/src/components/layout/nav.ts`
- Create: `frontend/src/components/layout/Sidebar.tsx`
- Create: `frontend/src/components/layout/Topbar.tsx`
- Create: `frontend/src/components/layout/AppShell.tsx`
- Test: `frontend/src/components/layout/Sidebar.test.tsx`

- [ ] **Step 1: Criar a config de navegação**

Create `frontend/src/components/layout/nav.ts`:

```ts
import {
  Boxes, Layers, Scissors, ClipboardList, Wallet, Shirt, Image, Settings,
  type LucideIcon,
} from 'lucide-react'

export type TabId =
  | 'fabric' | 'rolls' | 'cutting' | 'dtf-models'
  | 'dtf-stock' | 'separation' | 'financial' | 'config'

export interface NavItem {
  id: TabId
  label: string
  icon: LucideIcon
}

export interface NavGroup {
  label: string
  items: NavItem[]
}

export const NAV_GROUPS: NavGroup[] = [
  {
    label: 'Estoque',
    items: [
      { id: 'rolls', label: 'Bobinas', icon: Layers },
      { id: 'dtf-stock', label: 'Estoque DTF', icon: Boxes },
    ],
  },
  {
    label: 'Produção',
    items: [
      { id: 'cutting', label: 'Pedidos de Corte', icon: Scissors },
      { id: 'separation', label: 'Lista de Separação', icon: ClipboardList },
    ],
  },
  {
    label: 'Financeiro',
    items: [{ id: 'financial', label: 'Financeiro', icon: Wallet }],
  },
  {
    label: 'Cadastros',
    items: [
      { id: 'fabric', label: 'Tecidos', icon: Shirt },
      { id: 'dtf-models', label: 'Modelos DTF', icon: Image },
      { id: 'config', label: 'Configurações', icon: Settings },
    ],
  },
]

export const NAV_ITEMS: NavItem[] = NAV_GROUPS.flatMap((g) => g.items)

export function findNavItem(id: TabId): NavItem {
  return NAV_ITEMS.find((i) => i.id === id) ?? NAV_ITEMS[0]
}
```

> Nota: confirmar que cada ícone existe em `lucide-react@1.x` com `cd frontend && node -e "const l=require('lucide-react');['Boxes','Layers','Scissors','ClipboardList','Wallet','Shirt','Image','Settings'].forEach(n=>{if(!l[n])console.log('FALTA',n)})"`. Se algum faltar, substituir por um equivalente existente (ex.: `Package`, `FileText`).

- [ ] **Step 2: Escrever o teste do Sidebar**

Create `frontend/src/components/layout/Sidebar.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import { Sidebar } from './Sidebar'

test('marca o item ativo com aria-current', () => {
  render(<Sidebar active="financial" onSelect={() => {}} />)
  const ativo = screen.getByRole('button', { name: /financeiro/i })
  expect(ativo).toHaveAttribute('aria-current', 'page')
})
```

- [ ] **Step 3: Rodar e ver falhar**

Run: `cd frontend && pnpm test src/components/layout/Sidebar.test.tsx`
Expected: FAIL (módulo não existe).

- [ ] **Step 4: Implementar o Sidebar**

Create `frontend/src/components/layout/Sidebar.tsx`:

```tsx
import { NAV_GROUPS, type TabId } from './nav'
import { cn } from '@/lib/utils'

interface SidebarProps {
  active: TabId
  onSelect: (id: TabId) => void
  collapsed?: boolean
}

export function Sidebar({ active, onSelect, collapsed = false }: SidebarProps) {
  return (
    <nav className="flex h-full flex-col gap-5 p-3" aria-label="Navegação principal">
      {NAV_GROUPS.map((group) => (
        <div key={group.label} className="space-y-1">
          {!collapsed && (
            <p className="px-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
              {group.label}
            </p>
          )}
          {group.items.map((item) => {
            const Icon = item.icon
            const isActive = item.id === active
            return (
              <button
                key={item.id}
                type="button"
                onClick={() => onSelect(item.id)}
                aria-current={isActive ? 'page' : undefined}
                title={collapsed ? item.label : undefined}
                className={cn(
                  'flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-sm font-medium transition-colors',
                  collapsed && 'justify-center',
                  isActive
                    ? 'bg-accent/10 text-accent'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                )}
              >
                <Icon className="size-4 shrink-0" />
                {!collapsed && <span className="truncate">{item.label}</span>}
              </button>
            )
          })}
        </div>
      ))}
    </nav>
  )
}
```

- [ ] **Step 5: Rodar teste e ver passar**

Run: `cd frontend && pnpm test src/components/layout/Sidebar.test.tsx`
Expected: PASS.

- [ ] **Step 6: Implementar o Topbar**

Create `frontend/src/components/layout/Topbar.tsx`:

```tsx
import { Menu, PanelLeftClose, PanelLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from '@/components/ui/theme-toggle'

interface TopbarProps {
  title: string
  collapsed: boolean
  onToggleCollapse: () => void
  onOpenMobile: () => void
}

export function Topbar({ title, collapsed, onToggleCollapse, onOpenMobile }: TopbarProps) {
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
      <div className="ml-auto">
        <ThemeToggle />
      </div>
    </header>
  )
}
```

- [ ] **Step 7: Implementar o AppShell**

Create `frontend/src/components/layout/AppShell.tsx`:

```tsx
import { useState } from 'react'
import { Dialog, DialogContent } from '@/components/ui/dialog'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'
import { findNavItem, type TabId } from './nav'
import { cn } from '@/lib/utils'

interface AppShellProps {
  active: TabId
  onSelect: (id: TabId) => void
  children: React.ReactNode
}

export function AppShell({ active, onSelect, children }: AppShellProps) {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem('sidebar-collapsed') === '1')
  const [mobileOpen, setMobileOpen] = useState(false)

  const toggleCollapse = () => {
    setCollapsed((c) => {
      const next = !c
      localStorage.setItem('sidebar-collapsed', next ? '1' : '0')
      return next
    })
  }

  const handleSelect = (id: TabId) => {
    onSelect(id)
    setMobileOpen(false)
  }

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <aside
        className={cn(
          'hidden shrink-0 border-r border-border bg-card transition-[width] duration-200 lg:block',
          collapsed ? 'w-16' : 'w-60',
        )}
      >
        <div className="flex h-14 items-center gap-2 border-b border-border px-4">
          <div className="size-6 rounded bg-primary" />
          {!collapsed && <span className="text-sm font-semibold tracking-tight">StockShirt</span>}
        </div>
        <Sidebar active={active} onSelect={handleSelect} collapsed={collapsed} />
      </aside>

      <Dialog open={mobileOpen} onOpenChange={setMobileOpen}>
        <DialogContent className="left-0 top-0 h-screen max-w-[15rem] translate-x-0 translate-y-0 rounded-none border-y-0 border-l-0 p-0">
          <div className="flex h-14 items-center border-b border-border px-4">
            <span className="text-sm font-semibold tracking-tight">StockShirt</span>
          </div>
          <Sidebar active={active} onSelect={handleSelect} />
        </DialogContent>
      </Dialog>

      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar
          title={findNavItem(active).label}
          collapsed={collapsed}
          onToggleCollapse={toggleCollapse}
          onOpenMobile={() => setMobileOpen(true)}
        />
        <main className="flex-1 overflow-y-auto">
          <div className="mx-auto w-full max-w-7xl px-4 py-6 sm:px-6 lg:px-8">{children}</div>
        </main>
      </div>
    </div>
  )
}
```

> Nota: o `DialogContent` atual provavelmente centraliza com `fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2`. As classes acima sobrescrevem para encostar à esquerda. Se o `cn`/merge não vencer por especificidade, na Task 6 garantir que o posicionamento do `DialogContent` use classes que `twMerge` consiga sobrescrever (mesmos prefixos `left-`, `top-`, `translate-`).

- [ ] **Step 8: Commit**

```bash
git add frontend/src/components/layout/
git commit -m "feat: app shell com sidebar agrupada, topbar e menu mobile"
```

---

## Task 12: Ligar o AppShell no App.tsx

**Files:**
- Modify: `frontend/src/App.tsx`

- [ ] **Step 1: Reescrever o App.tsx**

Substituir `frontend/src/App.tsx` por:

```tsx
import { useState } from 'react'
import { AppShell } from '@/components/layout/AppShell'
import type { TabId } from '@/components/layout/nav'
import { FabricTypePage } from '@/features/settings/fabric/FabricTypePage'
import { FabricRollPage } from '@/features/fabric/rolls/FabricRollPage'
import { CuttingOrderPage } from '@/features/cutting/orders/CuttingOrderPage'
import { DtfModelPage } from '@/features/settings/dtf/DtfModelPage'
import { DtfStockPage } from '@/features/stock/dtf/DtfStockPage'
import { AppConfigPage } from '@/features/settings/config/AppConfigPage'
import { SeparationListPage } from '@/features/separation/SeparationListPage'
import { FinancialPage } from '@/features/financial/FinancialPage'

const PAGES: Record<TabId, React.ReactNode> = {
  fabric: <FabricTypePage />,
  rolls: <FabricRollPage />,
  cutting: <CuttingOrderPage />,
  'dtf-models': <DtfModelPage />,
  'dtf-stock': <DtfStockPage />,
  separation: <SeparationListPage />,
  financial: <FinancialPage />,
  config: <AppConfigPage />,
}

function App() {
  const [tab, setTab] = useState<TabId>('financial')
  return (
    <AppShell active={tab} onSelect={setTab}>
      {PAGES[tab]}
    </AppShell>
  )
}

export default App
```

- [ ] **Step 2: Typecheck + lint + rodar o app**

Run: `cd frontend && pnpm exec tsc --noEmit && pnpm lint`
Expected: sem erros.
Run manual: `pnpm dev` e abrir `http://localhost:5173` — confirmar sidebar agrupada, troca de telas, toggle de tema, colapso e menu mobile (reduzir a janela).

- [ ] **Step 3: Commit**

```bash
git add frontend/src/App.tsx
git commit -m "feat: substituir abas no topo pelo AppShell com sidebar"
```

---

## Task 13: Vitrine — refatorar SummaryCards para StatCard

**Files:**
- Modify: `frontend/src/features/financial/components/SummaryCards.tsx`

- [ ] **Step 1: Reescrever o SummaryCards usando StatCard**

Substituir `frontend/src/features/financial/components/SummaryCards.tsx` por:

```tsx
import type { FinancialSummaryDto } from '../types'
import { formatBRL } from '../format'
import { StatCard } from '@/components/ui/stat-card'

interface Props {
  summary: FinancialSummaryDto
}

export function SummaryCards({ summary }: Props) {
  const costs: { label: string; value: number }[] = [
    { label: 'Tecido', value: summary.totalFabric },
    { label: 'Corte', value: summary.totalCutting },
    { label: 'Costura', value: summary.totalSewing },
    { label: 'Defeitos', value: summary.totalDefects },
    { label: 'DTF', value: summary.totalDtf },
    { label: 'Receitas', value: summary.totalIncome },
  ]

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-6">
        {costs.map((c) => (
          <StatCard key={c.label} label={c.label} value={formatBRL(c.value)} />
        ))}
      </div>

      <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
        <StatCard
          label="Saldo do período"
          value={formatBRL(summary.balance)}
          tone={summary.balance >= 0 ? 'success' : 'danger'}
          hint={`Receitas ${formatBRL(summary.totalIncome)} − Despesas ${formatBRL(summary.totalExpense)}`}
        />
        <StatCard
          label="Custo médio / camiseta"
          value={summary.averageCostPerShirt != null ? formatBRL(summary.averageCostPerShirt) : '—'}
          hint="(Tecido + Corte + Costura) ÷ peças boas"
        />
        <StatCard
          label="Peças boas produzidas"
          value={summary.goodPiecesProduced}
          hint="no período selecionado"
        />
      </div>
    </div>
  )
}
```

- [ ] **Step 2: Typecheck + commit**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros.
```bash
git add frontend/src/features/financial/components/SummaryCards.tsx
git commit -m "feat: KPIs financeiros com StatCard e tom semantico no saldo"
```

---

## Task 14: Vitrine — refatorar EntriesTable para o primitivo Table

**Files:**
- Modify: `frontend/src/features/financial/components/EntriesTable.tsx`

- [ ] **Step 1: Ler o arquivo inteiro**

Run: `cd frontend && cat src/features/financial/components/EntriesTable.tsx`
Anotar: usa `<table>` cru, `Badge variant="default"/"secondary"`, empty state inline, cores `text-red-*`/`bg-neutral-*`, e um `Dialog` de confirmação de estorno com `Button`.

- [ ] **Step 2: Reescrever usando Table + Badge semântico + EmptyState + DialogFooter**

Reescrever `EntriesTable.tsx` preservando **toda a lógica** (`toReverse`, `onReverse`, `isReversing`, flags `isReversal`/`isReversed`). Versão:

```tsx
import { useState } from 'react'
import { Inbox } from 'lucide-react'
import type { FinancialEntryDto } from '../types'
import { formatBRL, formatDate } from '../format'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell,
} from '@/components/ui/table'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'

interface Props {
  entries: FinancialEntryDto[]
  onReverse: (id: string) => void
  isReversing: boolean
}

export function EntriesTable({ entries, onReverse, isReversing }: Props) {
  const [toReverse, setToReverse] = useState<FinancialEntryDto | null>(null)

  if (entries.length === 0) {
    return (
      <EmptyState
        icon={Inbox}
        title="Nenhum lançamento"
        description="Nenhum lançamento encontrado para os filtros selecionados."
      />
    )
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Data</TableHead>
            <TableHead>Categoria</TableHead>
            <TableHead>Descrição</TableHead>
            <TableHead className="text-right">Valor</TableHead>
            <TableHead className="text-right">Ação</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {entries.map((e) => (
            <TableRow key={e.id} className={e.isReversal ? 'bg-muted/40' : undefined}>
              <TableCell className="whitespace-nowrap text-muted-foreground">
                {formatDate(e.entryDate)}
              </TableCell>
              <TableCell>
                <Badge variant={e.type === 'Income' ? 'success' : 'neutral'}>{e.category}</Badge>
              </TableCell>
              <TableCell className="text-foreground">
                {e.description}
                {e.isReversal && <span className="ml-2 text-xs text-muted-foreground">(estorno)</span>}
                {e.isReversed && !e.isReversal && (
                  <span className="ml-2 text-xs text-danger">(estornado)</span>
                )}
              </TableCell>
              <TableCell
                className={`whitespace-nowrap text-right font-medium tabular-nums ${
                  e.amount < 0 ? 'text-danger' : 'text-foreground'
                }`}
              >
                {formatBRL(e.amount)}
              </TableCell>
              <TableCell className="text-right">
                {!e.isReversal && !e.isReversed && (
                  <Button variant="ghost" size="sm" onClick={() => setToReverse(e)}>
                    Estornar
                  </Button>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={toReverse !== null} onOpenChange={(o) => !o && setToReverse(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Estornar lançamento</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Confirma o estorno de “{toReverse?.description}” no valor de{' '}
            {toReverse ? formatBRL(toReverse.amount) : ''}? Esta ação cria um lançamento de
            estorno e não pode ser desfeita.
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setToReverse(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              isLoading={isReversing}
              onClick={() => {
                if (toReverse) onReverse(toReverse.id)
                setToReverse(null)
              }}
            >
              Estornar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}
```

> **Importante:** confira o arquivo original (Step 1) e preserve exatamente as mesmas condições de exibição do botão "Estornar" e os mesmos campos do DTO. Se o original calcular o valor/ação de modo diferente, mantenha a lógica do original — só troque a apresentação.

- [ ] **Step 3: Typecheck**

Run: `cd frontend && pnpm exec tsc --noEmit`
Expected: sem erros. Se `FinancialEntryDto` não tiver algum campo usado (ex.: `type`), ajustar para os campos reais vistos no Step 1.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/features/financial/components/EntriesTable.tsx
git commit -m "feat: tabela de lancamentos com primitivo Table e estados"
```

---

## Task 15: Vitrine — PageHeader e skeletons na FinancialPage

**Files:**
- Modify: `frontend/src/features/financial/FinancialPage.tsx`

- [ ] **Step 1: Ler a página inteira**

Run: `cd frontend && cat src/features/financial/FinancialPage.tsx`
Anotar: como abre o formulário de novo lançamento, estados de loading dos hooks (`useFinancialSummary`, `useFinancialEntries`), e estrutura atual (título, filtros).

- [ ] **Step 2: Aplicar PageHeader + Skeleton sem mexer na lógica de dados**

Editar `FinancialPage.tsx`:
- Substituir o título atual por:
  ```tsx
  <PageHeader
    title="Financeiro"
    description="Controle de custos, receitas e saldo do período."
    actions={/* botão/ação primária de novo lançamento já existente */}
  />
  ```
  (importar `import { PageHeader } from '@/components/ui/page-header'`)
- Onde hoje exibe texto de "carregando", trocar por skeletons:
  ```tsx
  import { Skeleton } from '@/components/ui/skeleton'
  // KPIs carregando:
  <div className="grid grid-cols-2 gap-3 md:grid-cols-3 lg:grid-cols-6">
    {Array.from({ length: 6 }).map((_, i) => (
      <Skeleton key={i} className="h-20 w-full" />
    ))}
  </div>
  ```
- Envolver o conteúdo num container vertical: `<div className="space-y-6"> ... </div>`.
- **Não** alterar chamadas de hooks, filtros, mutations ou parâmetros.

- [ ] **Step 3: Typecheck + lint**

Run: `cd frontend && pnpm exec tsc --noEmit && pnpm lint`
Expected: sem erros.

- [ ] **Step 4: Rodar o app e validar a vitrine**

Run: `pnpm dev` → abrir Financeiro. Confirmar: PageHeader, KPIs em StatCard, tabela premium, empty state ao filtrar sem resultado, skeleton no carregamento, dark mode ok.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/features/financial/FinancialPage.tsx
git commit -m "feat: FinancialPage com PageHeader e skeletons de carregamento"
```

---

## Task 16: Verificação final da fundação + vitrine

**Files:** nenhum (verificação)

- [ ] **Step 1: Suite completa**

Run: `cd frontend && pnpm exec tsc --noEmit && pnpm lint && pnpm test && pnpm build`
Expected: tudo verde.

- [ ] **Step 2: Checagem manual cruzada**

Run: `cd frontend && grep -rn "neutral-\|indigo-\|amber-\|fuchsia-\|emerald-\|bg-white\|text-black" src/components src/features/financial || echo "sem cores hardcoded na fundacao/vitrine"`
Expected: idealmente sem resultados nos primitivos e na tela Financeiro (cores agora vêm de tokens). Resíduos nas outras 7 telas são esperados (rollout posterior).

- [ ] **Step 3: Commit de fechamento (se houver ajustes)**

```bash
git add -A
git commit -m "chore: verificacao final da fundacao e vitrine financeira"
```

---

## Rollout posterior (fora deste plano)

Após aprovação da vitrine, aplicar o mesmo padrão (PageHeader + primitivos + estados + remoção de cores hardcoded) nas 7 telas restantes: Tecidos, Bobinas, Pedidos de Corte, Modelos DTF, Estoque DTF, Lista de Separação, Configurações. Cada tela vira seu próprio bloco de tarefas.

---

## Self-Review (preenchido pelo autor do plano)

- **Cobertura do spec:** tokens light/dark (T1) ✓; Inter (T1) ✓; tema+toggle (T2) ✓; Button/Input/Badge/Dialog (T3–T6) ✓; Card/Table/PageHeader/EmptyState/Skeleton/StatCard (T7–T10) ✓; sidebar agrupada + shell + mobile + a11y `aria-current` (T11–T12) ✓; remoção App.css (T1) ✓; vitrine Financeiro com KPIs/tabela/estados (T13–T15) ✓; verificação (T16) ✓.
- **Placeholders:** nenhum "TBD"; passos de código trazem o código completo; os passos que dependem de ler arquivo existente (Dialog T6, EntriesTable T14, FinancialPage T15) instruem ler antes e preservar a lógica real.
- **Consistência de tipos:** `TabId` definido em `nav.ts` e reusado em `Sidebar`/`AppShell`/`App.tsx`; `StatCard` tone `default|success|danger` usado coerentemente em SummaryCards; Badge variantes `success|neutral|danger` coerentes com EntriesTable.
- **Riscos conhecidos:** (a) nomes de ícones lucide v1.x — Step de verificação em T11; (b) sobrescrita de posicionamento do DialogContent para o menu mobile — nota em T11/T6; (c) campos reais de `FinancialEntryDto` — instrução de conferência em T14.
