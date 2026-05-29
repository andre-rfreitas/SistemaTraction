namespace SistemaTraction.Application.Dtf.DTOs;

public record DtfModelDto(
    Guid Id,
    string Name,
    string SheetLabel,
    int StampsPerSheet,
    decimal SheetCost
);
