using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Accounts;

/// <summary>
/// Xpollens account endpoints.
/// GET api/v3.0/accounts?accountHolder={appUserId} — list accounts for a user (paged envelope)
/// GET api/v3.0/accounts/{accountId} — get a single account
/// </summary>
internal sealed record XpollensAccountDto(
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("accountType")] string? AccountType,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("iban")] string? Iban,
    [property: JsonPropertyName("accountStatus")] string? AccountStatus,
    [property: JsonPropertyName("accountingBalance")] decimal AccountingBalance);

internal sealed record AccountPagedResponseDto(
    [property: JsonPropertyName("values")] List<XpollensAccountDto>? Values);

public sealed class XpollensAccountRepository(HttpClient httpClient, ILogger<XpollensAccountRepository> logger) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetAccountsAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching accounts for {AppUserId}", appUserId);
        var paged = await httpClient.GetFromJsonAsync<AccountPagedResponseDto>(
            $"api/v3.0/accounts?accountHolder={Uri.EscapeDataString(appUserId)}", ct);
        return (paged?.Values ?? []).Select(Map).ToList().AsReadOnly();
    }

    public async Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching account {AccountId}", accountId);
        var dto = await httpClient.GetFromJsonAsync<XpollensAccountDto>($"api/v3.0/accounts/{accountId}", ct);
        return dto is null ? null : Map(dto);
    }

    private static Account Map(XpollensAccountDto dto) => new(
        dto.AccountId,
        null,  // label is not provided by the Xpollens accounts API
        ParseType(dto.AccountType),
        dto.AccountingBalance,
        dto.Currency,
        dto.Iban,
        ParseStatus(dto.AccountStatus));

    private static AccountType ParseType(string? t) => t?.ToLowerInvariant() switch
    {
        "current" or "checking" => AccountType.Current,
        "savings" => AccountType.Savings,
        _ => AccountType.Unknown
    };

    private static AccountStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "active" or "activated" => AccountStatus.Active,
        "closed" => AccountStatus.Closed,
        "blocked" => AccountStatus.Blocked,
        _ => AccountStatus.Unknown
    };
}
