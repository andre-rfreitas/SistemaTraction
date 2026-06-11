namespace SistemaTraction.Application.Supplies.DTOs;

public record RegisterSupplyMovementResult(
    Guid MovementId,
    bool RequiresFinancialConfirmation,
    string? SuggestedDescription,
    decimal? SuggestedAmount = null);
