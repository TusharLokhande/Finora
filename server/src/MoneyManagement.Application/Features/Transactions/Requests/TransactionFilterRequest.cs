using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Transactions.Requests;

public class TransactionFilterRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    /// <summary>Matches either side of a transfer.</summary>
    public List<Guid>? AccountIds { get; set; }

    /// <summary>Matches each category itself and its sub-categories.</summary>
    public List<Guid>? CategoryIds { get; set; }

    public TransactionType? Type { get; set; }
    public long? MinAmount { get; set; }
    public long? MaxAmount { get; set; }

    /// <summary>Case-insensitive match on description or notes.</summary>
    public string? Search { get; set; }
}
