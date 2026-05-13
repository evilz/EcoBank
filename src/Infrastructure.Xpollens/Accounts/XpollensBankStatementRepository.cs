using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Accounts;

/// <summary>
/// Xpollens bank statement endpoints.
/// GET api/v3.0/bank-statements — list bank statements (paged envelope)
/// GET api/v3.0/bank-statements/{bankStatementId} — retrieve a bank statement by identifier
/// </summary>
internal sealed record BankStatementDto(
    [property: JsonPropertyName("bankStatementId")] string BankStatementId,
    [property: JsonPropertyName("accountId")] string? AccountId,
    [property: JsonPropertyName("periodStart")] DateTimeOffset? PeriodStart,
    [property: JsonPropertyName("periodEnd")] DateTimeOffset? PeriodEnd,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("documentUrl")] string? DocumentUrl);

internal sealed record BankStatementPagedDto(
    [property: JsonPropertyName("values")] List<BankStatementDto>? Values);

public sealed class XpollensBankStatementRepository(HttpClient httpClient, ILogger<XpollensBankStatementRepository> logger) : IBankStatementRepository
{
    public async Task<IReadOnlyList<BankStatement>> ListBankStatementsAsync(string? accountId, CancellationToken ct = default)
    {
        var url = "api/v3.0/bank-statements";
        if (!string.IsNullOrWhiteSpace(accountId))
            url += $"?accountId={Uri.EscapeDataString(accountId)}";

        logger.LogDebug("Listing bank statements for accountId={AccountId}", accountId);
        var paged = await httpClient.GetFromJsonAsync<BankStatementPagedDto>(url, ct);
        return (paged?.Values ?? []).Select(Map).ToList().AsReadOnly();
    }

    public async Task<BankStatement?> GetBankStatementAsync(string bankStatementId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching bank statement {BankStatementId}", bankStatementId);
        var dto = await httpClient.GetFromJsonAsync<BankStatementDto>($"api/v3.0/bank-statements/{bankStatementId}", ct);
        return dto is null ? null : Map(dto);
    }

    private static BankStatement Map(BankStatementDto dto) => new(
        dto.BankStatementId,
        dto.AccountId,
        dto.PeriodStart,
        dto.PeriodEnd,
        dto.Label,
        dto.DocumentUrl);
}
