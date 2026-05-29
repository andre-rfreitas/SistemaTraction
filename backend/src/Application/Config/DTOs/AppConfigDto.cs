namespace SistemaTraction.Application.Config.DTOs;

public record AppConfigDto(
    string Key,
    string Value,
    string? Description
);
