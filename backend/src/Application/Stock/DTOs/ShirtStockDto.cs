namespace SistemaTraction.Application.Stock.DTOs;

public record ShirtStockGridDto(
    string[] Sizes,
    List<ShirtStockRowDto> Rows,
    Dictionary<string, int> TotalsPerSize,
    int GrandTotal,
    int AlertThreshold
);

public record ShirtStockRowDto(
    Guid ColorId,
    string ColorName,
    Dictionary<string, int> Quantities,
    int Total
);

public record ShirtStockMovementsDto(
    List<ShirtStockMovementDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record ShirtStockMovementDto(
    Guid Id,
    DateTime Date,
    string FabricColorName,
    string Size,
    string ModelCode,
    int Delta,
    string Reason,
    string Origin
);

public record AdjustShirtStockResult(
    Guid MovementId,
    string FabricColorName,
    string Size,
    int Delta,
    int NewQuantity
);

public record ShirtStockItemDto(
    Guid Id,
    Guid FabricColorId,
    string FabricColorName,
    string Size,
    string ModelCode,
    int Quantity
);
