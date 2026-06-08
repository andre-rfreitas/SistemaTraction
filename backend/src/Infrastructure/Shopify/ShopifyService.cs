using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Infrastructure.Shopify;

public class ShopifyService(IApplicationDbContext context, IHttpClientFactory httpClientFactory) : IShopifyService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<ShopifySyncResultDto> SyncOrdersAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var configs = await context.AppConfigs
            .Where(c => c.Key.StartsWith("shopify_"))
            .ToListAsync(ct);

        var storeUrl = configs.FirstOrDefault(c => c.Key == "shopify_store_url")?.Value ?? "";
        var accessToken = configs.FirstOrDefault(c => c.Key == "shopify_access_token")?.Value ?? "";
        var apiVersion = configs.FirstOrDefault(c => c.Key == "shopify_api_version")?.Value ?? "2026-01";

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException(
                "Integração Shopify não configurada. Acesse Configurações > Shopify.");

        if (string.IsNullOrWhiteSpace(storeUrl))
            throw new InvalidOperationException(
                "URL da loja Shopify não configurada. Acesse Configurações > Shopify.");

        var baseUrl = storeUrl.StartsWith("http") ? storeUrl : $"https://{storeUrl}";

        var client = httpClientFactory.CreateClient("shopify");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-Shopify-Access-Token");
        client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

        var minDate = from.ToString("yyyy-MM-dd") + "T00:00:00-03:00";
        var maxDate = to.ToString("yyyy-MM-dd") + "T23:59:59-03:00";

        var initialPath = $"admin/api/{apiVersion}/orders.json"
            + $"?status=any&financial_status=paid"
            + $"&created_at_min={Uri.EscapeDataString(minDate)}"
            + $"&created_at_max={Uri.EscapeDataString(maxDate)}"
            + $"&limit=250&fields=id,order_number,total_price,source_name,created_at";

        var orders = new List<ShopifyOrder>();
        var errors = new List<string>();

        string? nextUrl = initialPath;
        while (nextUrl is not null)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(nextUrl, ct);
            }
            catch (Exception ex)
            {
                errors.Add($"Erro ao chamar API Shopify: {ex.Message}");
                break;
            }

            if (!response.IsSuccessStatusCode)
            {
                errors.Add($"Shopify retornou HTTP {(int)response.StatusCode}.");
                break;
            }

            var payload = await response.Content.ReadFromJsonAsync<ShopifyOrdersPayload>(JsonOptions, ct);
            if (payload?.Orders is not null)
                orders.AddRange(payload.Orders);

            nextUrl = ParseNextLink(response);
        }

        var imported = 0;
        var skipped = 0;
        var totalAmount = 0m;

        foreach (var order in orders)
        {
            try
            {
                var referenceId = GuidV5.Create(GuidV5.ShopifyOrderNamespace, order.Id.ToString());

                var exists = await context.FinancialEntries
                    .AnyAsync(e => e.ReferenceType == "ShopifyOrder" && e.ReferenceId == referenceId, ct);

                if (exists)
                {
                    skipped++;
                    continue;
                }

                var amount = decimal.Parse(order.TotalPrice, CultureInfo.InvariantCulture);
                var category = "Venda " + (order.SourceName ?? "web");
                var description = $"Pedido #{order.OrderNumber}";
                var entryDate = order.CreatedAt.UtcDateTime;

                var entry = FinancialEntry.CreateIncome(category, amount, description, entryDate,
                    referenceId, "ShopifyOrder");

                context.FinancialEntries.Add(entry);
                imported++;
                totalAmount += amount;
            }
            catch (Exception ex)
            {
                errors.Add($"Pedido #{order.OrderNumber}: {ex.Message}");
            }
        }

        if (imported > 0)
            await context.SaveChangesAsync(ct);

        await UpdateSyncMetadataAsync(imported, totalAmount, ct);

        return new ShopifySyncResultDto(imported, skipped, totalAmount, errors);
    }

    private async Task UpdateSyncMetadataAsync(int imported, decimal amount, CancellationToken ct)
    {
        var keys = new[] { "shopify_last_sync", "shopify_last_sync_imported", "shopify_last_sync_amount" };
        var configs = await context.AppConfigs.Where(c => keys.Contains(c.Key)).ToListAsync(ct);

        void Set(string key, string value)
        {
            var cfg = configs.FirstOrDefault(c => c.Key == key);
            if (cfg is not null) cfg.UpdateValue(value);
        }

        Set("shopify_last_sync", DateTime.UtcNow.ToString("O"));
        Set("shopify_last_sync_imported", imported.ToString());
        Set("shopify_last_sync_amount", amount.ToString(CultureInfo.InvariantCulture));

        await context.SaveChangesAsync(ct);
    }

    private static string? ParseNextLink(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Link", out var values))
            return null;

        foreach (var header in values)
        {
            foreach (var part in header.Split(','))
            {
                var trimmed = part.Trim();
                if (!trimmed.Contains("rel=\"next\""))
                    continue;

                var urlStart = trimmed.IndexOf('<') + 1;
                var urlEnd = trimmed.IndexOf('>');
                if (urlStart > 0 && urlEnd > urlStart)
                    return trimmed[urlStart..urlEnd];
            }
        }

        return null;
    }

    private record ShopifyOrdersPayload(
        [property: JsonPropertyName("orders")] List<ShopifyOrder>? Orders
    );

    private record ShopifyOrder(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("order_number")] int OrderNumber,
        [property: JsonPropertyName("total_price")] string TotalPrice,
        [property: JsonPropertyName("source_name")] string? SourceName,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
    );
}
