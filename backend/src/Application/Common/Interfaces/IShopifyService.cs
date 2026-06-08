using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IShopifyService
{
    Task<ShopifySyncResultDto> SyncOrdersAsync(DateOnly from, DateOnly to, CancellationToken ct);
}
