namespace MoneyManagement.Application.Features.Reports.Dto;

/// <summary>Everything past the top N, aggregated.</summary>
public class OtherSpendDto
{
    public int CategoryCount { get; set; }
    public long Spent { get; set; }
    public decimal SharePercent { get; set; }
}
