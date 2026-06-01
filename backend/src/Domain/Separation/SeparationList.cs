using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Separation;

public class SeparationList : BaseEntity
{
    public string FileName { get; private set; } = "";
    public DateTime UploadedAt { get; private set; }
    public SeparationListStatus Status { get; private set; }

    private readonly List<SeparationItem> _items = [];
    public IReadOnlyCollection<SeparationItem> Items => _items.AsReadOnly();

    private SeparationList() { }

    public static SeparationList Create(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Nome do arquivo é obrigatório.");

        return new SeparationList
        {
            FileName = fileName.Trim(),
            UploadedAt = DateTime.UtcNow,
            Status = SeparationListStatus.Pending
        };
    }

    public void Confirm()
    {
        if (Status != SeparationListStatus.Pending)
            throw new DomainException("Apenas listas pendentes podem ser confirmadas.");

        Status = SeparationListStatus.Confirmed;
        TouchUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == SeparationListStatus.Confirmed)
            throw new DomainException("Listas confirmadas não podem ser canceladas.");

        Status = SeparationListStatus.Cancelled;
        TouchUpdatedAt();
    }

    internal void AddItem(SeparationItem item) => _items.Add(item);
}
