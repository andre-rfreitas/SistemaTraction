using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetShopifyStatus;

public class GetShopifyStatusQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShopifyStatusQuery, ShopifyStatusDto>
{
    public async Task<ShopifyStatusDto> Handle(GetShopifyStatusQuery request, CancellationToken cancellationToken)
    {
        var configs = await context.AppConfigs
            .Where(c => c.Key.StartsWith("shopify_"))
            .ToListAsync(cancellationToken);

        var token = configs.FirstOrDefault(c => c.Key == "shopify_access_token")?.Value ?? "";
        var configured = !string.IsNullOrWhiteSpace(token);

        var lastSyncRaw = configs.FirstOrDefault(c => c.Key == "shopify_last_sync")?.Value ?? "";
        DateTime? lastSync = DateTime.TryParse(lastSyncRaw, out var dt) ? dt : null;

        var importedRaw = configs.FirstOrDefault(c => c.Key == "shopify_last_sync_imported")?.Value ?? "0";
        int.TryParse(importedRaw, out var lastSyncImported);

        var amountRaw = configs.FirstOrDefault(c => c.Key == "shopify_last_sync_amount")?.Value ?? "0";
        decimal.TryParse(amountRaw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var lastSyncAmount);

        return new ShopifyStatusDto(configured, lastSync, lastSyncImported, lastSyncAmount);
    }
}
