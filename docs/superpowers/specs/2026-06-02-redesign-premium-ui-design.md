# Redesign Premium UI — Sistema Traction (StockShirt)

**Data:** 2026-06-02
**Tipo:** Redesign visual / UX / arquitetura de componentes (sem mudança de regra de negócio)
**Status:** Aprovado para implementação

## Objetivo

Transformar a camada visual do frontend (React 19 + TS + Tailwind v4) num produto premium
comparável a Linear / Stripe / Vercel / Attio, eliminando a aparência de "admin template /
shadcn padrão / projeto de IA". Apenas redesign, UX e arquitetura visual.

## Restrições (NÃO alterar)

Regras de negócio, integrações, APIs, hooks de dados, schemas Zod, autenticação, banco,
permissões e fluxos internos permanecem intactos. Mexer só em: tokens de design, layout/shell,
primitivos de UI e a camada de apresentação das telas. Nenhuma lib de roteamento ou de estado
nova. Única dependência adicionada: `@fontsource-variable/inter` (fonte self-hosted, aprovada).

## Decisões travadas

1. **Sequência:** fundação (tokens + shell + primitivos) + tela **Financeiro** como vitrine
   primeiro; depois replicar nas outras 7 telas.
2. **Navegação:** sai abas-no-topo, entra **sidebar lateral agrupada**. Mantém `useState<Tab>`
   em `App.tsx` (sem router novo) — só muda a apresentação.
3. **Dark mode:** tokens em CSS variables suportando light **e** dark desde já; **light é o
   padrão** + toggle funcional (classe `.dark` no `<html>`, persistido em `localStorage`).

## Fase 1 — Auditoria (estado atual)

- `App.tsx`: 8 abas horizontais, título `text-2xl`, container `max-w-5xl` centralizado.
- Sem design tokens: cores cravadas no JSX (`neutral-900`, `indigo-600`, `amber-600`,
  `fuchsia-600`...). Ex.: `features/financial/components/SummaryCards.tsx`.
- `index.css` só tem `@import "tailwindcss"`; sem variáveis CSS; sem dark mode.
- Primitivos shadcn padrão sem customização (`button/input/badge/dialog/label`).
- `App.css`: boilerplate morto do Vite (`.hero`, `.vite`, `#next-steps`) — remover.
- Tabelas / cards / empty states reinventados ad-hoc em cada feature.
- Ponto positivo: código limpo e bem organizado (features colocalizadas, cva, RHF+Zod).

## Fase 2 — Design System

### Cores ("ink + 1 accent")
CSS variables no `index.css` via padrão Tailwind v4, com bloco `.dark`. Tokens semânticos
(estilo shadcn) mapeados em `@theme inline`:
`--background --foreground --card --card-foreground --muted --muted-foreground --border
--input --ring --primary --primary-foreground --secondary --secondary-foreground --accent
--accent-foreground --destructive --destructive-foreground` + status
`--success --warning --danger --info` (cada um com `-foreground`/`-subtle`).

- Neutros: escala cinza levemente fria (slate-tinted), não `neutral` puro.
- `--primary` = "ink" (quase-preto no light, quase-branco no dark) para ações primárias.
- `--accent`/`--ring` = índigo discreto (~`#4f46e5` dessaturado): nav ativa, links, foco.
- Semânticas dessaturadas substituem o arco-íris atual dos KPIs financeiros.
- Primitivos passam a usar `bg-background text-foreground border-border` etc. (nada hardcoded).

### Tipografia
`@fontsource-variable/inter` importado em `main.tsx`; `--font-sans` = Inter + fallback system.
Escala consistente: `display / h1 / h2 / h3 / body / small / caption` com line-heights fixos.
`tabular-nums` em todo valor numérico/financeiro.

### Espaçamento / Bordas / Sombras
- Grid de 8px: padding de página, gaps de seção e de cards padronizados (sem valores arbitrários).
- `--radius: 0.625rem` com `sm/md/lg` derivados; sem `rounded-full` em cards.
- Sombras suaves em camadas (low-alpha, estilo Linear/Stripe): `shadow-sm/md/lg`.

## Fase 3 — App Shell

Layout `sidebar + main` substitui o container centralizado.

- **Sidebar** ~240px, colapsável para rail de ícones (estado em `localStorage`). Grupos:
  - **Estoque** — Bobinas · Estoque DTF
  - **Produção** — Pedidos de Corte · Lista de Separação
  - **Financeiro** — Financeiro
  - **Cadastros** — Tecidos · Modelos DTF · Configurações
  - Ícones lucide, item ativo com accent + `aria-current="page"`. Rodapé: `ThemeToggle`.
- **Topbar** fina: título da página + slot de ação primária (via `PageHeader`).
- **Mobile**: sidebar vira slide-over (Radix Dialog já presente) acionado por hambúrguer;
  `main` sem coluna estreita — `max-w` generoso e responsivo.
- Mapeamento dos 8 tabs → grupos é só apresentação; nenhuma rota/lógica muda.

## Fase 4 — Componentes

**Refatorar (para tokens):** `Button` (+ estado `loading`/`isLoading` com spinner),
`Input` (+ estados de foco e erro), `Badge` (variantes `neutral/success/warning/danger/info`),
`Dialog` (espaçamento, `DialogFooter`).

**Criar:**
- `Card` (`Card/CardHeader/CardTitle/CardDescription/CardContent/CardFooter`)
- `Table` (`Table/TableHeader/TableBody/TableRow/TableHead/TableCell`) — estilo Stripe/Linear
- `PageHeader` (título + descrição opcional + slot de ações)
- `EmptyState` (ícone + título + descrição + ação opcional)
- `Skeleton` (loading)
- `StatCard` (KPI: label, valor `tabular-nums`, hint, tom semântico opcional)
- `ThemeToggle` + provider de tema (classe no `<html>`, `localStorage`, respeita `prefers-color-scheme` na 1ª visita)
- `AppShell` / `Sidebar` / `Topbar`

## Fase 5–8 — UX, Responsividade, Microinterações, A11y

- Estados padronizados: **loading** (Skeleton), **empty** (EmptyState), **error**, **success**
  (confirmações discretas, sem libs novas de toast — usar padrão simples inline).
- Responsivo mobile → tablet → desktop → ultrawide, layout pensado por breakpoint.
- Transições 150–200ms; hover/active refinados; sem efeitos extravagantes.
- WCAG AA: contraste adequado, foco visível via `--ring`, navegação por teclado,
  `aria-current`/`aria-label` onde necessário, `prefers-reduced-motion`.

## Fase 9 — Refatoração visual

Remover `App.css` (boilerplate morto). Eliminar cores/estilos duplicados migrando para tokens
e primitivos compartilhados. Padronizar classes Tailwind.

## Vitrine — Tela Financeiro

Reconstruída como referência visual a ser replicada:
- `PageHeader` com título + ação primária ("Novo lançamento").
- KPIs em `StatCard` (neutros; **uma** cor semântica só no saldo; `tabular-nums`).
- `CategoryChart` restilizado com tokens.
- `EntriesTable` sobre o novo `Table` + `Badge` semântico + `EmptyState` + `Skeleton`.
- `ManualEntryForm` com `Input`/`Button` atualizados e estados de erro.

## Critérios de aceitação

- `pnpm typecheck && pnpm lint` passam.
- Nenhuma mudança em hooks, schemas, `lib/api.ts` ou tipos de dados.
- Light e dark funcionais via toggle; sem cor hardcoded nos primitivos.
- Sidebar agrupada navegando as 8 telas; `App.css` removido.
- Tela Financeiro entregue no novo padrão; demais telas seguem na fase de rollout.

## Rollout (após vitrine aprovada)

Aplicar PageHeader + primitivos + estados nas 7 telas restantes: Tecidos, Bobinas,
Pedidos de Corte, Modelos DTF, Estoque DTF, Lista de Separação, Configurações.
