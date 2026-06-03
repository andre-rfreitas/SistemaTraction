using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfStockMovementDto(
    Guid Id,
    DtfMovementType Type,
    int Delta,
    string? Reason,
    DateTime CreatedAt,
    int? SheetCount
);
