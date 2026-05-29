using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Config;

public class AppConfig : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private AppConfig() { }

    public static AppConfig Create(string key, string value, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Chave de configuração não pode ser vazia.");

        return new AppConfig
        {
            Key = key.Trim(),
            Value = value?.Trim() ?? string.Empty,
            Description = description?.Trim()
        };
    }

    public void UpdateValue(string value)
    {
        Value = value?.Trim() ?? string.Empty;
        TouchUpdatedAt();
    }
}
