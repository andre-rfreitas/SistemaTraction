namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfStockItemDetailDto(
    DtfStockItemDto Item,
    List<DtfStockMovementDto> Movements
);
