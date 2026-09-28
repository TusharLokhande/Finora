using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Categories.Dto;

public class CategoryDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool Active { get; set; }
    public string? DefaultKey { get; set; }
    public List<CategoryDto> Children { get; set; } = [];
}
