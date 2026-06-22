namespace SistemaTraction.Application.Separation.DTOs;

public record SeparationListSummaryDto(
    Guid Id,
    string FileName,
    DateTime UploadedAt,
    string Status,
    int ItemCount,
    int TotalQuantity
);

public record SeparationItemDto(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    int Quantity,
    int SortOrder
);

public record SeparationListDetailDto(
    Guid Id,
    string FileName,
    DateTime UploadedAt,
    string Status,
    List<SeparationItemDto> Items
);

// Stock check DTOs
public record ShirtStockCheckDto(
    string ModelCode,
    string Color,
    string Size,
    int Needed,
    int Available,
    bool Ok
);

public record StockCheckResultDto(
    List<ShirtStockCheckDto> ShirtChecks,
    bool CanConfirm
);

// Confirm result
public record SeparationConfirmResultDto(
    Guid SeparationListId,
    List<ShirtDeductionDto> ShirtDeductions
);

public record ShirtDeductionDto(string ModelCode, string Color, string Size, int Quantity);
