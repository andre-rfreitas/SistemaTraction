# Estoque DTF por estampa — Design

**Data:** 2026-06-03
**Status:** Aprovado para planejamento

## Problema

Hoje o estoque DTF é controlado por **folha**: `DtfStockItem.CurrentQuantity` e
`DtfStockMovement.Delta` representam folhas, e o atributo `DtfModel.StampsPerSheet`
existe mas não influencia o estoque. Na prática, uma folha contém várias estampas e
o consumo real (produção/venda) acontece por estampa, não por folha. O controle por
folha não reflete a quantidade de estampas realmente disponível.

## Objetivo

Tornar **estampa** a unidade canônica do estoque DTF. Todas as quantidades, cálculos,
movimentações, entradas, baixas e exibições passam a ser em estampas.

Exemplo de referência:
- Entrada de **5 folhas**, modelo com **10 estampas/folha** → estoque grava **50 estampas**.
- Consumo de **3 estampas** → estoque cai para **47 estampas**.

## Decisões tomadas

1. **Entrada digitada em folhas, convertida para estampas.** O usuário informa o número
   de folhas recebidas; o sistema multiplica por `DtfModel.StampsPerSheet` e grava o
   resultado em estampas.
2. **Saída e Ajuste sempre em estampas.** Consumo, produção, venda e correções de
   inventário descontam/ajustam diretamente em estampas, sem conversão.
3. **Sem migração de dados existentes.** Não há estoque DTF real em produção; a migration
   cobre apenas mudanças de schema e novo seed de config.
4. **Guardar o nº de folhas na entrada** (`SheetCount`) para o histórico mostrar a origem
   ("5 folhas → +50 estampas").
5. **Threshold de alerta dedicado** `dtf_stock_alert_threshold` (default 100 estampas),
   separado do `stock_alert_threshold` compartilhado com o estoque de camisetas.

## Arquitetura

### Unidade canônica
- `DtfStockItem.CurrentQuantity` → **estampas**.
- `DtfStockMovement.Delta` → **estampas** (assinado: positivo entrada, negativo saída).
- `DtfModel.StampsPerSheet` → fator de conversão folha→estampa.

### Domínio (`backend/src/Domain/Dtf`)

**`DtfStockMovement`**
- Novo campo `int? SheetCount` (nullable). Preenchido apenas em movimentos de Entrada
  (nº de folhas recebidas). `null` em Saída e Ajuste.
- `Delta` permanece em estampas.
- Factory `Create` passa a aceitar `sheetCount`.

**`DtfStockItem.AddMovement`**
- Continua append-only e mantém a regra "estoque nunca negativo" (agora em estampas).
- Assinatura passa a receber o delta **já em estampas** mais um `sheetCount` opcional:
  `AddMovement(DtfMovementType type, int quantityStamps, string? reason, int? sheetCount = null)`.
- A conversão folha→estampa acontece na camada de aplicação (que conhece o `DtfModel`),
  não no domínio — o domínio só recebe e valida estampas.

**`DtfMovementType`**
- Comentários atualizados: `Entrada` = estampas recebidas (via folhas), `Saida` =
  estampas consumidas, `Ajuste` = correção em estampas.

### Aplicação (`backend/src/Application/Dtf`)

**`RegisterDtfMovementCommand`** — mantém um único comando `(DtfModelId, Type, Quantity, Reason)`.
A semântica de `Quantity` depende de `Type`:
- **Entrada** → `Quantity` = nº de **folhas** (> 0).
- **Saída** → `Quantity` = **estampas** (> 0).
- **Ajuste** → `Quantity` = delta em **estampas** (≠ 0, pode ser negativo).

**`RegisterDtfMovementCommandHandler`**
- Carrega o `DtfModel` (já valida existência).
- Para Entrada: `stamps = Quantity * model.StampsPerSheet`, chama
  `AddMovement(Entrada, stamps, reason, sheetCount: Quantity)`.
- Para Saída/Ajuste: chama `AddMovement(type, Quantity, reason)` (sheetCount null).

**`RegisterDtfMovementCommandValidator`** — inalterado na estrutura (Quantity ≠ 0; > 0 para
Entrada/Saída). As mensagens permanecem genéricas.

**DTOs**
- `DtfStockItemDto` ganha `int StampsPerSheet` (para a UI exibir equivalência em folhas).
- `DtfStockMovementDto` ganha `int? SheetCount`.
- Query handlers `GetDtfStockItems` e `GetDtfStockItemByModel` projetam os novos campos.

### Alerta de estoque baixo

- Nova config `dtf_stock_alert_threshold`, default `"100"`, com descrição
  "Quantidade mínima de estampas DTF antes de disparar alerta de reposição".
- Adicionada ao seed (`AppConfigConfiguration`) e à migration. Não altera o
  `stock_alert_threshold` existente.

### Frontend (`frontend/src/features/stock/dtf` e `.../settings/dtf`)

**`RegisterMovementForm`**
- Entrada: label "Folhas recebidas"; preview ao vivo "= N estampas" usando
  `stampsPerSheet` do modelo selecionado.
- Saída/Ajuste: label "Estampas".
- Textos das opções do select atualizados (folhas só na Entrada).

**`DtfStockList`**
- Número principal em **estampas** (label "estampas"); subtítulo "≈ X folhas"
  (`Math.floor(qty / stampsPerSheet)`).
- Alerta de estoque baixo usa `dtf_stock_alert_threshold`.

**`DtfStockDetail` (histórico)**
- Entrada: "+50 estampas (5 folhas)".
- Saída/Ajuste: só estampas.

**`DtfStockPage`** e demais textos: "posição em estampas".

**Types TS** (`stock/dtf/types.ts`): `DtfStockItemDto.stampsPerSheet`,
`DtfStockMovementDto.sheetCount`.

### Banco de dados
- Migration EF Core adicionando coluna `SheetCount` (nullable int) em
  `DtfStockMovements` e o seed da config `dtf_stock_alert_threshold`.
- Configuração EF de `DtfStockMovement` atualizada para o novo campo.

## Fluxo de dados (Entrada)

1. Usuário seleciona modelo (com `stampsPerSheet`) e digita folhas na UI.
2. UI mostra preview `folhas × stampsPerSheet`.
3. POST `/dtf-stock/movements` com `{ type: Entrada, quantity: folhas }`.
4. Handler converte para estampas, grava `Delta` (estampas) e `SheetCount` (folhas).
5. `CurrentQuantity` cresce em estampas; lista e histórico refletem estampas.

## Regras de negócio (mantidas/atualizadas)

- Estoque DTF nunca negativo — validado no domínio, em estampas.
- Movimentação append-only — nunca atualizar, só inserir.
- Conversão folha→estampa só na entrada; consumo sempre em estampas.

## Testes

- `DtfStockTests` atualizado para a nova semântica. O modelo de teste tem
  `StampsPerSheet = 9`; uma Entrada de `N` folhas resulta em `N*9` estampas.
- Novos casos: conversão correta folha→estampa na Entrada; `SheetCount` preenchido na
  Entrada e `null` em Saída/Ajuste; Saída/Ajuste operando direto em estampas.

## Fora de escopo

- Conversão de dados históricos (não há dados reais).
- Consumo automático de estoque DTF a partir de produção/separação (hoje a Saída é
  manual; permanece manual).
- Vínculo de custo (`SheetCost`) a lançamentos financeiros na entrada (não existe hoje).
