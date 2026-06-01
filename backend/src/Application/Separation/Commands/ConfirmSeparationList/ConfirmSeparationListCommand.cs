using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.ConfirmSeparationList;

public record ConfirmSeparationListCommand(Guid SeparationListId) : IRequest<SeparationConfirmResultDto>;
