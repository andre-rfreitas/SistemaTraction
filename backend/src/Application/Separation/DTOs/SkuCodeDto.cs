namespace SistemaTraction.Application.Separation.DTOs;

public record SkuCodeDto(Guid Id, string Code, string Value, string Category, Guid? DtfModelId);
