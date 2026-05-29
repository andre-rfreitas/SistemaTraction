using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetFinancialEntries;

public record GetFinancialEntriesQuery(
    string? Category = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<List<FinancialEntryDto>>;
