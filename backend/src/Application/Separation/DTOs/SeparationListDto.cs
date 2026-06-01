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
    Guid? DtfModelId,
    string? DtfModelName,
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
    string Color,
    string Size,
    int Needed,
    int Available,
    bool Ok
);

public record DtfStockCheckDto(
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int StampsPerSheet,
    decimal SheetCost,
    int Needed,
    int Available,
    bool FromStock,
    int SheetsToOrder,
    int StampsFromSheets,
    int Surplus,
    decimal OrderCost
);

public record StockCheckResultDto(
    List<ShirtStockCheckDto> ShirtChecks,
    List<DtfStockCheckDto> DtfChecks,
    decimal TotalDtfCost,
    bool CanConfirm
);

// Confirm result
public record SeparationConfirmResultDto(
    Guid SeparationListId,
    List<ShirtDeductionDto> ShirtDeductions,
    List<DtfUsageDto> DtfUsages,
    List<DtfOrderDto> DtfOrders,
    decimal TotalDtfCost,
    string? WhatsAppMessage,
    string? WaMeLink,
    string DtfSupplierName,
    string DtfSupplierPhone
);

public record ShirtDeductionDto(string Color, string Size, int Quantity);

public record DtfUsageDto(string ModelName, int QuantityUsed);

public record DtfOrderDto(
    string ModelName,
    string SheetLabel,
    int StampsPerSheet,
    int SheetsOrdered,
    decimal SheetCost,
    decimal TotalCost
);
