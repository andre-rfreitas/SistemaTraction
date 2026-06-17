namespace SistemaTraction.Application.Cutting.DTOs;

public record CuttingRecommendationDto(
    string ColorName,
    int DaysUsed,
    int BasedOnOrders,
    Dictionary<string, int> RecommendedPieces,
    Dictionary<string, int> DemandBySize,
    Dictionary<string, int> CurrentStockBySize,
    bool HasSufficientHistory
);

public record CuttingRecommendationHistoryItemDto(
    Guid CuttingOrderId,
    int OrderNumber,
    string FabricColorName,
    DateTime CreatedAt,
    int DaysUsed,
    int BasedOnOrders,
    Dictionary<string, int> RecommendedPieces,
    Dictionary<string, int> RequestedPieces,
    Dictionary<string, int>? ActualDeliveredPieces
);

public record CuttingOrderItemDto(
    Guid Id,
    Guid FabricRollId,
    string FabricTypeName,
    string FabricTypeVariation,
    string FabricColorName,
    string? FabricColorHexCode,
    decimal FabricRollWeightKg,
    Dictionary<string, int> RequestedPieces,
    int TotalPieces
);

public record CuttingOrderDto(
    Guid Id,
    int OrderNumber,
    List<CuttingOrderItemDto> Items,
    Dictionary<string, int> RequestedPieces,
    Dictionary<string, int>? DeliveredPieces,
    Dictionary<string, int>? SewingDeliveredPieces,
    int TotalPieces,
    string Status,
    DateTime? SentAt,
    string? Notes,
    DateTime CreatedAt
);

public record CreateCuttingOrderResult(
    Guid CuttingOrderId,
    int OrderNumber,
    string WhatsAppMessage,
    string? WaMeLink,
    string CutterPhone,
    string CutterName
);

public record SendCuttingOrderResult(
    bool Sent,
    string? WaMeLink,
    string? Error
);

public record CuttingDeliveryDto(
    Guid Id,
    Guid CuttingOrderId,
    Dictionary<string, int> DeliveredPieces,
    int TotalPieces,
    decimal CuttingCostTotal,
    DateTime DeliveredAt
);

public record RegisterCuttingDeliveryResult(
    Guid CuttingDeliveryId,
    int TotalPieces,
    decimal CuttingCostTotal,
    string WhatsAppMessage,
    string? WaMeLink,
    string SewerPhone,
    string SewerName
);

public record SewingItemResult(
    string FabricColorName,
    string FabricTypeName,
    string? FabricColorHexCode,
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces
);

public record RegisterSewingDeliveryResult(
    Guid SewingDeliveryId,
    int TotalGoodPieces,
    int TotalDefectivePieces,
    decimal SewingCostTotal,
    decimal DefectCostTotal,
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces,
    List<SewingItemResult> Items
);
