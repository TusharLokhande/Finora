using MoneyManagement.Domain.Common;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

public class Category : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? Color { get; set; }

    /// <summary>Lucide icon name (e.g. "utensils"), rendered client-side via lucide-react.</summary>
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
    public string? DefaultKey { get; set; }

    public User? User { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
}
