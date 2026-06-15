using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfOrderItemDto(
    Guid Id,
    Guid DtfModelId,
    string ModelName,
    string SheetLabel,
    int SheetQuantity,
    int StampsPerSheet,
    int StampsTotal);

public record DtfOrderDto(
    Guid Id,
    int OrderNumber,
    DtfOrderStatus Status,
    string? Notes,
    DateTime? SentAt,
    DateTime? ReceivedAt,
    List<DtfOrderItemDto> Items,
    DateTime CreatedAt);
