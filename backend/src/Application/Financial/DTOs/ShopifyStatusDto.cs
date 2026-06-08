namespace SistemaTraction.Application.Financial.DTOs;

public record ShopifyStatusDto(
    bool Configured,
    DateTime? LastSync,
    int LastSyncImported,
    decimal LastSyncAmount
);
