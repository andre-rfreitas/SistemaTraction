namespace SistemaTraction.Application.Cutting.DTOs;

public record CuttingOrderDto(
    Guid Id,
    int OrderNumber,
    Guid FabricRollId,
    string FabricTypeName,
    string FabricTypeVariation,
    string FabricColorName,
    string? FabricColorHexCode,
    decimal FabricRollWeightKg,
    Dictionary<string, int> RequestedPieces,
    Dictionary<string, int>? DeliveredPieces,
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

public record RegisterSewingDeliveryResult(
    Guid SewingDeliveryId,
    int TotalGoodPieces,
    int TotalDefectivePieces,
    decimal SewingCostTotal,
    decimal DefectCostTotal,
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces
);
