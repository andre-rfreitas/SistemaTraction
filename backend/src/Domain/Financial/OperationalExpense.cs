using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Financial;

public class OperationalExpense : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal FixedMonthlyValue { get; private set; }
    public decimal RatePercent { get; private set; }
    public bool IsActive { get; private set; } = true;

    private OperationalExpense() { }

    public static OperationalExpense Create(string name, decimal fixedMonthlyValue, decimal ratePercent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da despesa operacional não pode ser vazio.");

        if (fixedMonthlyValue < 0)
            throw new DomainException("Valor fixo mensal não pode ser negativo.");

        if (ratePercent < 0 || ratePercent > 100)
            throw new DomainException("Taxa percentual deve estar entre 0 e 100.");

        if (fixedMonthlyValue == 0 && ratePercent == 0)
            throw new DomainException("Informe ao menos um valor fixo ou taxa percentual.");

        return new OperationalExpense
        {
            Name = name.Trim(),
            FixedMonthlyValue = fixedMonthlyValue,
            RatePercent = ratePercent,
        };
    }

    public void Update(string name, decimal fixedMonthlyValue, decimal ratePercent, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da despesa operacional não pode ser vazio.");

        if (fixedMonthlyValue < 0)
            throw new DomainException("Valor fixo mensal não pode ser negativo.");

        if (ratePercent < 0 || ratePercent > 100)
            throw new DomainException("Taxa percentual deve estar entre 0 e 100.");

        if (fixedMonthlyValue == 0 && ratePercent == 0)
            throw new DomainException("Informe ao menos um valor fixo ou taxa percentual.");

        Name = name.Trim();
        FixedMonthlyValue = fixedMonthlyValue;
        RatePercent = ratePercent;
        IsActive = isActive;
        TouchUpdatedAt();
    }
}
