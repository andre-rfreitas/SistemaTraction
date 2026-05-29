using MediatR;
using SistemaTraction.Application.Config.DTOs;

namespace SistemaTraction.Application.Config.Queries.GetAppConfigByKey;

public record GetAppConfigByKeyQuery(string Key) : IRequest<AppConfigDto?>;
