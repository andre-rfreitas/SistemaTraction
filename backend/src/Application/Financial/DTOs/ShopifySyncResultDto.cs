namespace SistemaTraction.Application.Financial.DTOs;

public record ShopifySyncResultDto(
    int TotalImported,
    int TotalSkipped,
    decimal TotalAmount,
    List<string> Errors
);
