using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSkuCodes;

public record GetSkuCodesQuery : IRequest<List<SkuCodeDto>>;
