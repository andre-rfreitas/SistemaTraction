using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetShopifyStatus;

public record GetShopifyStatusQuery : IRequest<ShopifyStatusDto>;
