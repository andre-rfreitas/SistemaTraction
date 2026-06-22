using System.Text.RegularExpressions;
using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SistemaTraction.Infrastructure.Pdf;

/// <summary>
/// Parser PDF com três estratégias em cascata:
///
/// 1) Cabeçalho "SKU" — lê a coluna "SKU" pela posição do cabeçalho (layout antigo com coluna nomeada).
///    SKUs que quebram em duas linhas na coluna estreita são remontados.
///
/// 2) Varredura direta por palavras — detecta palavras que casam com o padrão
///    MODELO-COR-TAMANHO (ex: BBL-BLK-M) e procura "× N" ou número próximo
///    na mesma linha como quantidade. Funciona para o PDF do UpSeller/Chrome onde
///    não existe cabeçalho "SKU".
///
/// 3) Varredura de texto completo — fallback regex sobre page.Text.
/// </summary>
public class PdfPigParser : IPdfParser
{
    // SKU: ao menos 3 partes maiúsculas separadas por hífen (MODELO-COR-TAMANHO)
    // Ex: BBL-BLK-M, REG-RED-GG, REG-RED-G1
    private static readonly Regex SkuRegex = new(
        @"(?<![A-Z0-9\-])[A-Z][A-Z0-9]{0,7}(?:-[A-Z0-9]{1,8}){2,4}(?![A-Z0-9\-])",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex QtyRegex    = new(@"^\s*(\d{1,4})\b",       RegexOptions.Compiled);
    private static readonly Regex DigitsOnly  = new(@"^\d{1,4}$",             RegexOptions.Compiled);

    // "× N", "x N", "xN" — símbolo de multiplicação antes do número
    private static readonly Regex CrossQtyRegex = new(@"[×xX]\s*(\d{1,4})",   RegexOptions.Compiled);

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);

            // Estratégia 1: coluna com cabeçalho "SKU"
            var items = ParseBySkuColumnHeader(doc);
            if (items.Count > 0)
                return new ParseResult(true, items, null);

            // Estratégia 2: varredura direta de palavras (UpSeller sem cabeçalho)
            var direct = ParseByDirectWordScan(doc);
            if (direct.Count > 0)
                return new ParseResult(true, direct, null);

            // Estratégia 3: varredura de texto completo (fallback)
            var fallback = ParseByFullTextScan(doc);
            if (fallback.Count > 0)
                return new ParseResult(true, fallback, null);

            return new ParseResult(false, [],
                "Nenhum item encontrado. Verifique se o PDF é uma lista de separação válida " +
                "e se os Códigos SKU estão configurados na aba 'Config. SKU'.");
        }
        catch (Exception ex)
        {
            return new ParseResult(false, [], $"Erro ao ler o PDF: {ex.Message}");
        }
    }

    // ── Estratégia 1: cabeçalho "SKU" ────────────────────────────────────────────
    private static List<ParsedItem> ParseBySkuColumnHeader(PdfDocument doc)
    {
        const double xTol    = 12.0;
        const double yTol    = 5.0;

        var rows = new List<(string Sku, int Qty)>();

        foreach (var page in doc.GetPages())
        {
            var words = GetNormalizedWords(page);

            var header = words.FirstOrDefault(w =>
                w.Text.Equals("SKU", StringComparison.OrdinalIgnoreCase));
            if (header.Text is null) continue;

            var skuX   = header.X;
            var headerY = header.Y;

            var skuColumn = words
                .Where(w => Math.Abs(w.X - skuX) <= xTol && w.Y > headerY + 1)
                .OrderBy(w => w.Y)
                .ToList();
            if (skuColumn.Count == 0) continue;

            // Remonta SKUs quebrados em duas linhas
            var skus = new List<(string Sku, double Y)>();
            foreach (var w in skuColumn)
            {
                if (w.Text.Contains('-') || skus.Count == 0)
                    skus.Add((w.Text, w.Y));
                else
                {
                    var last = skus[^1];
                    skus[^1] = (last.Sku + w.Text, last.Y);
                }
            }

            var qtyColumn = words
                .Where(w => w.X > skuX + 40 && DigitsOnly.IsMatch(w.Text))
                .OrderBy(w => w.Y)
                .ToList();

            var rowHeight = 50.0;
            if (skus.Count > 1)
            {
                var gaps = skus.Zip(skus.Skip(1), (a, b) => b.Y - a.Y).OrderBy(g => g).ToList();
                rowHeight = gaps[gaps.Count / 2];
            }

            for (var i = 0; i < skus.Count; i++)
            {
                var top    = skus[i].Y - yTol;
                var bottom = (i + 1 < skus.Count ? skus[i + 1].Y : skus[i].Y + rowHeight) - yTol;
                var qtyWord = qtyColumn.FirstOrDefault(q => q.Y >= top && q.Y < bottom);
                var qty = qtyWord.Text is not null && int.TryParse(qtyWord.Text, out var q) && q is > 0 and < 9_999
                    ? q : 1;
                rows.Add((skus[i].Sku, qty));
            }
        }

        return Accumulate(rows);
    }

    // ── Estratégia 2: varredura direta de palavras ────────────────────────────────
    //
    // Para o PDF do UpSeller: cada linha tem a forma:
    //   [SKU]   × [Qty]
    //
    // Algoritmo:
    // 1. Agrupa todas as palavras por faixa vertical (linha).
    // 2. Em cada linha procura palavras com o padrão SKU.
    // 3. Busca a quantidade no "×N" mais próximo na mesma linha,
    //    ou no número após o símbolo × em qualquer palavra da linha.
    private static List<ParsedItem> ParseByDirectWordScan(PdfDocument doc)
    {
        const double lineHeightTol = 6.0; // pixels de tolerância para agrupar numa linha

        var rows = new List<(string Sku, int Qty)>();

        foreach (var page in doc.GetPages())
        {
            var words = GetNormalizedWords(page);
            if (words.Count == 0) continue;

            // Agrupa por linha (Y similar)
            var lines = new List<List<(double Y, double X, string Text)>>();
            foreach (var w in words.OrderBy(w => w.Y).ThenBy(w => w.X))
            {
                var matched = false;
                foreach (var line in lines)
                {
                    if (Math.Abs(line[0].Y - w.Y) <= lineHeightTol)
                    {
                        line.Add(w);
                        matched = true;
                        break;
                    }
                }
                if (!matched) lines.Add([(w.Y, w.X, w.Text)]);
            }

            foreach (var line in lines)
            {
                // Junta as palavras da linha em ordem de X para facilitar o regex
                var lineText = string.Join(" ", line.OrderBy(w => w.X).Select(w => w.Text));

                // Encontra todas as palavras da linha que são SKUs
                var skuMatches = SkuRegex.Matches(lineText);
                if (skuMatches.Count == 0) continue;

                // Tenta extrair a quantidade: procura "× N" ou "x N" na linha
                var qty = 1;
                var crossMatch = CrossQtyRegex.Match(lineText);
                if (crossMatch.Success && int.TryParse(crossMatch.Groups[1].Value, out var cq) && cq is > 0 and < 9_999)
                {
                    qty = cq;
                }
                else
                {
                    // Fallback: pega o último número puro na linha
                    var numbers = line
                        .OrderBy(w => w.X)
                        .Where(w => DigitsOnly.IsMatch(w.Text))
                        .ToList();
                    if (numbers.Count > 0 && int.TryParse(numbers[^1].Text, out var nq) && nq is > 0 and < 9_999)
                        qty = nq;
                }

                foreach (Match m in skuMatches)
                    rows.Add((m.Value.Trim(), qty));
            }
        }

        return Accumulate(rows);
    }

    // ── Estratégia 3: varredura de texto completo ─────────────────────────────────
    private static List<ParsedItem> ParseByFullTextScan(PdfDocument doc)
    {
        var rows = new List<(string Sku, int Qty)>();

        foreach (var page in doc.GetPages())
        {
            var text = NormalizeHyphens(page.Text ?? "");
            foreach (Match m in SkuRegex.Matches(text))
                rows.Add((m.Value.Trim(), ExtractQtyAfter(text, m.Index + m.Length)));
        }

        return Accumulate(rows);
    }

    private static int ExtractQtyAfter(string text, int start)
    {
        if (start >= text.Length) return 1;
        var segment = text[start..Math.Min(start + 20, text.Length)];
        // Tenta "× N" primeiro
        var cross = CrossQtyRegex.Match(segment);
        if (cross.Success && int.TryParse(cross.Groups[1].Value, out var cq) && cq is > 0 and < 9_999)
            return cq;
        // Fallback: número simples
        var m = QtyRegex.Match(segment);
        return m.Success && int.TryParse(m.Groups[1].Value, out var q) && q is > 0 and < 9_999 ? q : 1;
    }

    // ── Utilitários ───────────────────────────────────────────────────────────────

    private static List<(double Y, double X, string Text)> GetNormalizedWords(Page page) =>
        page.GetWords()
            .Select(w => (
                Y: page.Height - w.BoundingBox.Bottom,
                X: w.BoundingBox.Left,
                Text: NormalizeHyphens(w.Text).Trim()))
            .Where(w => w.Text.Length > 0)
            .ToList();

    private static List<ParsedItem> Accumulate(List<(string Sku, int Qty)> rows)
    {
        var order  = new List<string>();
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (sku, qty) in rows)
        {
            if (!totals.ContainsKey(sku)) order.Add(sku);
            totals[sku] = totals.GetValueOrDefault(sku) + qty;
        }

        return order
            .Select((sku, idx) => new ParsedItem(sku, "", "", totals[sku], idx))
            .ToList();
    }

    // Substitui variantes Unicode de hífen pelo hífen ASCII padrão
    private static string NormalizeHyphens(string text) =>
        Regex.Replace(text, @"[­‑‒–—−﹣－]", "-");
}
