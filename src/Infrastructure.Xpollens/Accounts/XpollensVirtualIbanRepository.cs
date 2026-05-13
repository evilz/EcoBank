using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Accounts;

internal sealed record VirtualIbanPagedDto([property: JsonPropertyName("values")] List<VirtualIbanDto>? Values);

internal sealed record VirtualIbanDto(
    [property: JsonPropertyName("virtualIbanId")] string VirtualIbanId,
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("virtualIban")] string? Iban,
    [property: JsonPropertyName("virtualIbanHint")] string? IbanHint,
    [property: JsonPropertyName("virtualIbanStatus")] string? Status);

public sealed class XpollensVirtualIbanRepository(HttpClient httpClient, ILogger<XpollensVirtualIbanRepository> logger) : IVirtualIbanRepository
{
    public async Task<IReadOnlyList<VirtualIban>> GetVirtualIbansAsync(string accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching virtual IBANs for {AccountId}", accountId);
        var response = await httpClient.GetFromJsonAsync<VirtualIbanPagedDto>(
            $"api/v3.0/virtual-ibans?accountId={Uri.EscapeDataString(accountId)}&limit=100", ct);
        return (response?.Values ?? []).Select(Map).ToList().AsReadOnly();
    }

    public async Task<VirtualIban> CreateVirtualIbanAsync(string accountId, string? label, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v3.0/virtual-ibans", new { accountId, label }, ct);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<VirtualIbanDto>(cancellationToken: ct);
        return dto is null
            ? new VirtualIban("", accountId, "", label, AccountStatus.Unknown)
            : Map(dto);
    }

    private static VirtualIban Map(VirtualIbanDto dto) => new(
        dto.VirtualIbanId,
        dto.AccountId,
        dto.Iban ?? dto.IbanHint ?? "",
        null,
        dto.Status?.ToLowerInvariant() == "active" ? AccountStatus.Active : AccountStatus.Unknown);
}
