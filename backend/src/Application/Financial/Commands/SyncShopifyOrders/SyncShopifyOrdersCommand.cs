using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Commands.SyncShopifyOrders;

public record SyncShopifyOrdersCommand(DateOnly From, DateOnly To) : IRequest<ShopifySyncResultDto>;
