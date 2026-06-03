namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfStockItemDto(
    Guid Id,
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int CurrentQuantity,
    int StampsPerSheet
);
