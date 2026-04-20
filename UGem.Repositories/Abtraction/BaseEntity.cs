namespace UGem.Repositories.Abtraction;

public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;

    public bool IsDeleted { get; set; } // Soft Delete, Tránh xung đột khóa ngoại (Foreign Key Constraint)
}
