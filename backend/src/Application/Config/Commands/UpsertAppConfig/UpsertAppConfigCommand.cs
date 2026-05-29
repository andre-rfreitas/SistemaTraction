using MediatR;

namespace SistemaTraction.Application.Config.Commands.UpsertAppConfig;

public record UpsertAppConfigCommand(string Key, string Value) : IRequest;
