namespace SistemaTraction.Application.Stock.DTOs;

public record GenericProductCategoryDto(Guid Id, string Name);

public record GenericProductDto(Guid Id, Guid CategoryId, string Name, int Quantity);

public record GenericProductMovementDto(
    Guid Id,
    DateTime Date,
    string ProductName,
    int Delta,
    string Reason,
    string Origin
);

public record GenericProductMovementsResponseDto(
    List<GenericProductMovementDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
