using MediatR;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Commands.RenameSeparationList;

public record RenameSeparationListCommand(Guid Id, string FileName) : IRequest<SeparationListSummaryDto>;
