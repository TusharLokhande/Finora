namespace MoneyManagement.Application.Features.Categories.Requests;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
