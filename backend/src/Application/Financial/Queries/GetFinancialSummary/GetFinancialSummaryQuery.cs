using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetFinancialSummary;

public record GetFinancialSummaryQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<FinancialSummaryDto>;
