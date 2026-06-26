# Novo formato de SKU na Lista de Separação — Modelagem-Estampa-Cor-Tamanho

## Contexto

O upload de lista de separação hoje assume SKUs no formato `MODELO-COR-TAMANHO`
(3 segmentos, ex: `BBL-BLK-M`). O ERP/marketplace (UpSeller) passou a gerar SKUs
no formato `MODELAGEM-ESTAMPA-COR-TAMANHO` (4 segmentos, ex: `REG-MADT-RED-G`),
conforme o PDF "Lista de Resumo" usado para subir novas listas. Esse PDF tem as
colunas: **Anúncios | Variação | SKU do Anúncio | Qtd.**

Exemplos reais do PDF:
- `REG-ANGE-BLK-G1` → Modelagem=REG, Estampa=ANGE, Cor=BLK, Tamanho=G1
- `REG-MADT-RED-M` → Modelagem=REG, Estampa=MADT, Cor=RED, Tamanho=M
- `REG-TRAO-BLK-P` → Modelagem=REG, Estampa=TRAO, Cor=BLK, Tamanho=P

Alguns SKUs do mesmo PDF não seguem esse padrão (ex: `48581264638184`,
`CAM0015BASPREPG`) — esses continuam caindo no fallback existente (usar a
coluna "Variação" e/ou marcar campos como `"?"`), sem alteração de
comportamento.

O sistema precisa reconhecer o novo formato de 4 segmentos e expor o campo
"Estampa" na lista de separação, vinculando-o a um `DtfModel` já cadastrado
(módulo de Estampagem/DTF), para uso futuro — sem desconto automático de
estoque DTF por enquanto.

## Decisões

- **Compatibilidade retroativa:** SKU com 3 segmentos continua sendo
  interpretado como `MODELO-COR-TAMANHO` (comportamento atual, sem Estampa).
  SKU com 4+ segmentos passa a ser `MODELAGEM-ESTAMPA-COR-TAMANHO`. Segmentos
  extras (5º em diante) continuam ignorados.
- **Estampa não afeta estoque de camiseta.** A checagem/dedução de estoque ao
  confirmar a lista continua baseada em Modelagem (como `ModelCode`) + Cor +
  Tamanho, exatamente como hoje.
- **Estampa não desconta estoque de DTF automaticamente.** O vínculo
  (`DtfModelId`) fica apenas registrado no item, para uso futuro.
- **Configuração de código "Estampa"** não é texto livre: ao cadastrar um
  código SKU com categoria Estampa, o usuário escolhe um `DtfModel` já
  existente em um select. O `Value` salvo é o nome do `DtfModel` (para
  exibição/preview), e o `DtfModelId` é a referência real.

## Mudanças de domínio (backend)

### `SkuCodeCategory` (enum)
Adicionar valor `Estampa` (mantém `Modelo`, `Cor`, `Tamanho`).

### `SkuCode` (entidade)
- Novo campo `DtfModelId` (`Guid?`), preenchido apenas quando
  `Category == Estampa`. Nulo para as demais categorias.
- `Create`/`Update` passam a aceitar `dtfModelId` opcional; validar que só é
  informado quando `category == Estampa` (senão lançar `DomainException`).

### `SeparationItem` (entidade)
- Novo campo `Estampa` (`string`, não obrigatório — pode ficar vazio/`"?"`
  como já ocorre com Cor/Tamanho quando não resolvido).
- Novo campo já existente `DtfModelId` continua sendo usado via `SetDtfModel`,
  agora chamado automaticamente durante o upload quando a Estampa resolver.
- `Create`/`Update` passam a aceitar `estampa` (string).

### Migration EF Core
- Nova coluna `SkuCodes.DtfModelId` (nullable, FK para `DtfModels`, sem
  cascade — `OnDelete(DeleteBehavior.Restrict)` ou `SetNull`).
- Nova coluna `SeparationItems.Estampa` (`nvarchar`, nullable/default `""`).
- Atualizar enum `SkuCodeCategory` no banco (se armazenado como string, sem
  migração de dados necessária; se como int, garantir que `Estampa` não
  colide com valores existentes).

## Parsing do SKU (`UploadSeparationListCommandHandler.ResolveSkuParts`)

Lógica revisada:

```
parts = sku.Split('-')

if parts.Length >= 4:
    // Modelagem-Estampa-Cor-Tamanho
    estampaCode = parts[1] → lookup SkuCode (Category=Estampa) → (DtfModelId, Value/Nome)
    corCode     = parts[2] → lookup SkuCode (Category=Cor)     → Value
    tamanhoCode = parts[3] → lookup SkuCode (Category=Tamanho) → Value
elif parts.Length == 3:
    // legado: Modelo-Cor-Tamanho (sem Estampa)
    corCode     = parts[1] → lookup SkuCode (Category=Cor)     → Value
    tamanhoCode = parts[2] → lookup SkuCode (Category=Tamanho) → Value
    estampa = null
else:
    // sem segmentos suficientes — comportamento atual de fallback (Variação / "?")
```

- Fallback igual ao atual: se não houver mapeamento de código, usa o
  segmento bruto (maiúsculo) como valor.
- Após resolver, se `DtfModelId` foi encontrado, chamar
  `item.SetDtfModel(dtfModelId)` após `SeparationItem.Create(...)`.
- `ModelCode` (1º segmento, usado em `GetStockCheckQueryHandler` e
  `ConfirmSeparationListCommandHandler`) não muda — continua sendo
  `parts[0]`.

## DTOs / API

- `SeparationItemDto` (backend): adicionar `string Estampa`, `Guid? DtfModelId`.
- `SkuCodeDto` (backend): adicionar `Guid? DtfModelId`.
- `UpsertSkuCodeCommand`: adicionar `Guid? DtfModelId`.
- `UpsertSkuCodeCommandHandler`: validar `DtfModelId` exigido quando
  `Category == Estampa` e proibido nas demais categorias; resolver
  `DtfModel.Name` para popular `Value` automaticamente quando vier
  `DtfModelId`.
- Sem novos endpoints — apenas ampliação dos contratos existentes em
  `/separation-lists` e `/separation-lists/sku-codes`.

## Frontend

### `types.ts`
- `SeparationItemDto`: adicionar `estampa: string`, `dtfModelId: string | null`.
- `UpdateItemPayload`: adicionar `estampa: string`.
- `SkuCodeCategory`: adicionar `'Estampa'`.
- `SkuCodeDto`: adicionar `dtfModelId: string | null`.

### `SeparationListPage.tsx` (step `review`)
- Tabela passa de `SKU | Modelo | Cor | Tam. | Qtd` para
  `SKU | Modelagem | Estampa | Cor | Tam. | Qtd`.
- Coluna Estampa editável como input de texto (mesmo padrão de Cor/Tamanho
  hoje), atualizando `editedItems`.

### `SkuConfigPanel.tsx`
- `CATEGORIES` ganha `{ value: 'Estampa', label: 'Estampa' }`.
- `CATEGORY_VARIANT` ganha uma variante para Estampa.
- Quando `form.category === 'Estampa'`: o campo "Valor" some e em seu lugar
  aparece um `<select>` com a lista de `DtfModel` (via `useDtfModels`,
  reaproveitado de `features/settings/dtf/hooks/useDtfModels`). Selecionar um
  modelo define `form.dtfModelId` e `form.value` (nome do modelo, somente
  leitura/derivado).
- Texto de ajuda e exemplo do formato atualizados para
  `MODELAGEM-ESTAMPA-COR-TAMANHO` (ex: `REG-MADT-RED-G`).
- `SkuPreview`: ajustar inferência de categoria por posição para 4 segmentos
  (`0=Modelagem, 1=Estampa, 2=Cor, 3=Tamanho`), com fallback para o formato
  legado de 3 segmentos quando o SKU digitado tiver só 3 partes.

## Fora de escopo

- Dedução automática de estoque de DTF ao confirmar a lista de separação.
- Migração/retrofit de listas de separação já existentes (itens antigos sem
  Estampa continuam exibindo o campo vazio).
- Mudanças no parser de PDF (`PdfPigParser`) — a extração do SKU bruto já
  funciona para o padrão de 4 segmentos (regex já aceita 3–5 segmentos); só a
  interpretação semântica do SKU muda.
