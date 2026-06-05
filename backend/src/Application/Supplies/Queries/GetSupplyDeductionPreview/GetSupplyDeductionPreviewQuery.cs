using MediatR;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyDeductionPreview;

public record GetSupplyDeductionPreviewQuery(int OrderCount) : IRequest<List<SupplyDeductionPreviewItem>>;
