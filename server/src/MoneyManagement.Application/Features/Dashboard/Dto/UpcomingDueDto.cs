namespace MoneyManagement.Application.Features.Dashboard.Dto;

public class UpcomingDueDto
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public long Outstanding { get; set; }
    public long? CreditLimit { get; set; }
    public DateOnly DueDate { get; set; }

    /// <summary>Negative when overdue.</summary>
    public int DaysUntilDue { get; set; }
}
