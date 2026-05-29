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
