using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Transactions.Dto;

/// <summary>A past description plus what it was last logged with, for quick-add prefill.</summary>
public class DescriptionSuggestionDto
{
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid AccountId { get; set; }
}
