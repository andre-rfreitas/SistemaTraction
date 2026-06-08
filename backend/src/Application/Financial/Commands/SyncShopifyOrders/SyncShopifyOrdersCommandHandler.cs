using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Commands.SyncShopifyOrders;

public class SyncShopifyOrdersCommandHandler(IShopifyService shopifyService)
    : IRequestHandler<SyncShopifyOrdersCommand, ShopifySyncResultDto>
{
    public Task<ShopifySyncResultDto> Handle(SyncShopifyOrdersCommand request, CancellationToken cancellationToken)
        => shopifyService.SyncOrdersAsync(request.From, request.To, cancellationToken);
}
