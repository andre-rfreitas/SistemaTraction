using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.UpdateSeparationItems;

public record UpdateSeparationItemsCommand(
    Guid SeparationListId,
    List<UpdateItemDto> Items
) : IRequest<SeparationListDetailDto>;

public record UpdateItemDto(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    int Quantity
);
