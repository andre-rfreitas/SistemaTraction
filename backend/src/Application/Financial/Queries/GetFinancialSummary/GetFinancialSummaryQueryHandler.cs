using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Financial.Queries.GetFinancialSummary;

public class GetFinancialSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialSummaryQuery, FinancialSummaryDto>
{
    public async Task<FinancialSummaryDto> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = request.To ?? from.AddMonths(1).AddTicks(-1);

        var entries = await context.FinancialEntries
            .Where(e => e.EntryDate >= from && e.EntryDate <= to)
            .ToListAsync(cancellationToken);

        decimal CategoryTotal(string category) =>
            entries.Where(e => e.Category == category).Sum(e => e.Amount);

        var totalFabric = CategoryTotal(FinancialCategories.Fabric);
        var totalCutting = CategoryTotal(FinancialCategories.Cutting);
        var totalSewing = CategoryTotal(FinancialCategories.Sewing);
        var totalDefects = CategoryTotal(FinancialCategories.Defects);
        var totalDtf = CategoryTotal(FinancialCategories.Dtf);

        var totalIncome = entries.Where(e => e.Type == FinancialEntryType.Income).Sum(e => e.Amount);
        var totalExpense = entries.Where(e => e.Type == FinancialEntryType.Expense).Sum(e => e.Amount);

        var expenseByCategory = entries
            .Where(e => e.Type == FinancialEntryType.Expense)
            .GroupBy(e => e.Category)
            .Select(g => new CategoryTotalDto(g.Key, g.Sum(e => e.Amount)))
            .Where(c => c.Total != 0)
            .OrderByDescending(c => c.Total)
            .ToList();

        var sewingDeliveries = await context.SewingDeliveries
            .Where(s => s.DeliveredAt >= from && s.DeliveredAt <= to)
            .ToListAsync(cancellationToken);

        var goodPiecesProduced = sewingDeliveries.Sum(s => s.GetTotalGoodPieces());

        decimal? averageCostPerShirt = goodPiecesProduced > 0
            ? Math.Round((totalFabric + totalCutting + totalSewing) / goodPiecesProduced, 2)
            : null;

        // OPEX calculation
        var activeOpex = await context.OperationalExpenses
            .Where(e => !e.IsDeleted && e.IsActive)
            .ToListAsync(cancellationToken);

        var periodDays = Math.Max(1, (to - from).TotalDays);
        const double daysInMonth = 30.0;

        var opexItems = activeOpex.Select(e =>
        {
            var proratedFixed = e.FixedMonthlyValue * (decimal)(periodDays / daysInMonth);
            var rateAmount = e.RatePercent / 100m * totalIncome;
            var periodTotal = Math.Round(proratedFixed + rateAmount, 2);
            return new OpexPeriodItemDto(e.Id, e.Name, e.FixedMonthlyValue, e.RatePercent,
                Math.Round(proratedFixed, 2), Math.Round(rateAmount, 2), periodTotal);
        }).ToList();

        var totalOpex = opexItems.Sum(o => o.PeriodTotal);
        var balance = totalIncome - totalExpense - totalOpex;

        return new FinancialSummaryDto(
            from,
            to,
            totalFabric,
            totalCutting,
            totalSewing,
            totalDefects,
            totalDtf,
            totalIncome,
            totalExpense,
            balance,
            goodPiecesProduced,
            averageCostPerShirt,
            expenseByCategory,
            totalOpex,
            opexItems
        );
    }
}
