namespace SistemaTraction.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    public void MarkAsDeleted() => IsDeleted = true;
    public void TouchUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
