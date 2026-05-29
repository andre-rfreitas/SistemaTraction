using MediatR;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfModels;

public record GetDtfModelsQuery : IRequest<List<DtfModelDto>>;
