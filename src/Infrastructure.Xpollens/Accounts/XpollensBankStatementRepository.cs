using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Accounts;

/// <summary>
/// Xpollens bank statement endpoint.
/// GET api/v3.0/bank-statements/{bankStatementId} — retrieve a bank statement by identifier
/// </summary>
internal sealed record BankStatementDto(
    [property: JsonPropertyName("bankStatementId")] string BankStatementId,
    [property: JsonPropertyName("accountId")] string? AccountId,
    [property: JsonPropertyName("periodStart")] DateTimeOffset? PeriodStart,
    [property: JsonPropertyName("periodEnd")] DateTimeOffset? PeriodEnd,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("documentUrl")] string? DocumentUrl);

public sealed class XpollensBankStatementRepository(HttpClient httpClient, ILogger<XpollensBankStatementRepository> logger) : IBankStatementRepository
{
    public async Task<BankStatement?> GetBankStatementAsync(string bankStatementId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching bank statement {BankStatementId}", bankStatementId);
        var dto = await httpClient.GetFromJsonAsync<BankStatementDto>($"api/v3.0/bank-statements/{bankStatementId}", ct);
        return dto is null ? null : new BankStatement(
            dto.BankStatementId,
            dto.AccountId,
            dto.PeriodStart,
            dto.PeriodEnd,
            dto.Label,
            dto.DocumentUrl);
    }
}
