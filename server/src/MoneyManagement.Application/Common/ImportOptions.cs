namespace MoneyManagement.Application.Common;

public class ImportOptions
{
    public int BatchChunkSize { get; set; } = 200;
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
