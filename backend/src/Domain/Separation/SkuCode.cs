using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Separation;

public class SkuCode : BaseEntity
{
    public string Code { get; private set; } = "";
    public string Value { get; private set; } = "";
    public SkuCodeCategory Category { get; private set; }
    public Guid? DtfModelId { get; private set; }

    private SkuCode() { }

    public static SkuCode Create(string code, string value, SkuCodeCategory category, Guid? dtfModelId = null)
    {
        ValidateForCategory(code, value, category, dtfModelId);

        return new SkuCode
        {
            Code = code.Trim().ToUpper(),
            Value = value.Trim(),
            Category = category,
            DtfModelId = dtfModelId,
        };
    }

    public void Update(string code, string value, SkuCodeCategory category, Guid? dtfModelId = null)
    {
        ValidateForCategory(code, value, category, dtfModelId);

        Code = code.Trim().ToUpper();
        Value = value.Trim();
        Category = category;
        DtfModelId = dtfModelId;
        TouchUpdatedAt();
    }

    private static void ValidateForCategory(string code, string value, SkuCodeCategory category, Guid? dtfModelId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código SKU não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Valor do código SKU não pode ser vazio.");
        if (dtfModelId.HasValue && category != SkuCodeCategory.Estampa)
            throw new DomainException("Modelo DTF só pode ser informado para a categoria Estampa.");
        if (category == SkuCodeCategory.Estampa && !dtfModelId.HasValue)
            throw new DomainException("Selecione um modelo DTF para a categoria Estampa.");
    }
}
