using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Categories.Requests;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public Guid? ParentId { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
