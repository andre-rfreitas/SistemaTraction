using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Separation;

public class SkuCode : BaseEntity
{
    public string Code { get; private set; } = "";
    public string Value { get; private set; } = "";
    public SkuCodeCategory Category { get; private set; }

    private SkuCode() { }

    public static SkuCode Create(string code, string value, SkuCodeCategory category)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código SKU não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Valor do código SKU não pode ser vazio.");

        return new SkuCode
        {
            Code = code.Trim().ToUpper(),
            Value = value.Trim(),
            Category = category
        };
    }

    public void Update(string code, string value, SkuCodeCategory category)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código SKU não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Valor do código SKU não pode ser vazio.");

        Code = code.Trim().ToUpper();
        Value = value.Trim();
        Category = category;
        TouchUpdatedAt();
    }
}
