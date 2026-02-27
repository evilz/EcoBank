using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Accounts;

/// <summary>
/// Xpollens account endpoints.
/// GET api/v3.0/accounts — list accounts for the authenticated user
/// GET api/v3.0/accounts/{accountId} — get a single account
/// </summary>
internal sealed record AccountDto(
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("balance")] decimal Balance,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("iban")] string? Iban,
    [property: JsonPropertyName("status")] string? Status);

public sealed class XpollensAccountRepository(HttpClient httpClient, ILogger<XpollensAccountRepository> logger) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetAccountsAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Fetching accounts");
        var dtos = await httpClient.GetFromJsonAsync<List<AccountDto>>("api/v3.0/accounts", ct) ?? [];
        return dtos.Select(Map).ToList().AsReadOnly();
    }

    public async Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching account {AccountId}", accountId);
        var dto = await httpClient.GetFromJsonAsync<AccountDto>($"api/v3.0/accounts/{accountId}", ct);
        return dto is null ? null : Map(dto);
    }

    private static Account Map(AccountDto dto) => new(
        dto.AccountId,
        dto.Label,
        ParseType(dto.Type),
        dto.Balance,
        dto.Currency,
        dto.Iban,
        ParseStatus(dto.Status));

    private static AccountType ParseType(string? t) => t?.ToLowerInvariant() switch
    {
        "current" or "checking" => AccountType.Current,
        "savings" => AccountType.Savings,
        _ => AccountType.Unknown
    };

    private static AccountStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "active" => AccountStatus.Active,
        "closed" => AccountStatus.Closed,
        "blocked" => AccountStatus.Blocked,
        _ => AccountStatus.Unknown
    };
}
