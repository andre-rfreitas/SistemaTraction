using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Financial;

public class FinancialEntry : BaseEntity
{
    public FinancialEntryType Type { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public DateTime EntryDate { get; private set; }

    private FinancialEntry() { }

    public static FinancialEntry CreateExpense(
        string category,
        decimal amount,
        string description,
        Guid? referenceId = null,
        string? referenceType = null)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Categoria do lançamento financeiro não pode ser vazia.");

        if (amount <= 0)
            throw new DomainException("Valor do lançamento deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição do lançamento não pode ser vazia.");

        return new FinancialEntry
        {
            Type = FinancialEntryType.Expense,
            Category = category.Trim(),
            Amount = amount,
            Description = description.Trim(),
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            EntryDate = DateTime.UtcNow
        };
    }

    public static FinancialEntry CreateIncome(
        string category,
        decimal amount,
        string description,
        Guid? referenceId = null,
        string? referenceType = null)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Categoria do lançamento financeiro não pode ser vazia.");

        if (amount <= 0)
            throw new DomainException("Valor do lançamento deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição do lançamento não pode ser vazia.");

        return new FinancialEntry
        {
            Type = FinancialEntryType.Income,
            Category = category.Trim(),
            Amount = amount,
            Description = description.Trim(),
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            EntryDate = DateTime.UtcNow
        };
    }
}
