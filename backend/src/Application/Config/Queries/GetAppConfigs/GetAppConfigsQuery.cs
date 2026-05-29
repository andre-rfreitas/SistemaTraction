using MediatR;
using SistemaTraction.Application.Config.DTOs;

namespace SistemaTraction.Application.Config.Queries.GetAppConfigs;

public record GetAppConfigsQuery : IRequest<List<AppConfigDto>>;
