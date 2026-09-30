using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Features.Transactions.Requests;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUser;

    public TransactionsController(ITransactionService transactionService, ICurrentUserService currentUser)
    {
        _transactionService = transactionService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] PageRequest<TransactionFilterRequest> request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.SearchAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int limit = 8, CancellationToken cancellationToken = default)
    {
        var result = await _transactionService.GetRecentAsync(UserId, limit, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetDescriptionSuggestionsAsync(UserId, q ?? string.Empty, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Downloads matching transactions as .xlsx. An empty body exports everything.</summary>
    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] TransactionFilterRequest? filter, CancellationToken cancellationToken)
    {
        var stream = new MemoryStream();
        var result = await _transactionService.ExportAsync(UserId, filter, stream, cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult();

        stream.Position = 0;
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"transactions-{DateTime.UtcNow:yyyy-MM-dd}.xlsx");
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkTransactionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.BulkDeleteAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("bulk-restore")]
    public async Task<IActionResult> BulkRestore([FromBody] BulkTransactionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.BulkRestoreAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("bulk-recategorize")]
    public async Task<IActionResult> BulkRecategorize([FromBody] BulkRecategorizeRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.BulkRecategorizeAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.CreateAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.UpdateAsync(UserId, id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _transactionService.DeleteAsync(UserId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _transactionService.RestoreAsync(UserId, id, cancellationToken);
        return result.ToActionResult();
    }
}
