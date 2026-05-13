using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Payments;

internal sealed record BeneficiaryPagedDto(
    [property: JsonPropertyName("values")] List<BeneficiaryDto>? Values);

internal sealed record BeneficiaryDto(
    [property: JsonPropertyName("beneficiaryId")] string BeneficiaryId,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("iban")] string? Iban,
    [property: JsonPropertyName("bic")] string? Bic);

public sealed class XpollensBeneficiaryRepository(HttpClient httpClient, ILogger<XpollensBeneficiaryRepository> logger) : IBeneficiaryRepository
{
    public async Task<IReadOnlyList<Beneficiary>> GetBeneficiariesAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching beneficiaries for {AppUserId}", appUserId);
        var response = await httpClient.GetFromJsonAsync<BeneficiaryPagedDto>(
            $"api/v2.0/users/{Uri.EscapeDataString(appUserId)}/beneficiary?limit=100", ct);
        return (response?.Values ?? []).Select(Map).ToList().AsReadOnly();
    }

    public async Task<Beneficiary?> GetBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default)
    {
        var dto = await httpClient.GetFromJsonAsync<BeneficiaryDto>(
            $"api/v2.0/users/{Uri.EscapeDataString(appUserId)}/beneficiary/{Uri.EscapeDataString(beneficiaryId)}", ct);
        return dto is null ? null : Map(dto);
    }

    public async Task<Beneficiary> SaveBeneficiaryAsync(string appUserId, Beneficiary beneficiary, CancellationToken ct = default)
    {
        var dto = new BeneficiaryDto(beneficiary.BeneficiaryId, beneficiary.DisplayName, beneficiary.Iban, beneficiary.Bic);
        var path = string.IsNullOrWhiteSpace(beneficiary.BeneficiaryId)
            ? $"api/v2.0/users/{Uri.EscapeDataString(appUserId)}/beneficiary"
            : $"api/v2.0/users/{Uri.EscapeDataString(appUserId)}/beneficiary/{Uri.EscapeDataString(beneficiary.BeneficiaryId)}";
        var response = string.IsNullOrWhiteSpace(beneficiary.BeneficiaryId)
            ? await httpClient.PostAsJsonAsync(path, dto, ct)
            : await httpClient.PutAsJsonAsync(path, dto, ct);
        response.EnsureSuccessStatusCode();
        var saved = await response.Content.ReadFromJsonAsync<BeneficiaryDto>(cancellationToken: ct);
        return saved is null ? beneficiary : Map(saved);
    }

    public async Task DeleteBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync(
            $"api/v2.0/users/{Uri.EscapeDataString(appUserId)}/beneficiary/{Uri.EscapeDataString(beneficiaryId)}", ct);
        response.EnsureSuccessStatusCode();
    }

    private static Beneficiary Map(BeneficiaryDto dto) => new(
        dto.BeneficiaryId,
        string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.Iban ?? "Bénéficiaire" : dto.DisplayName,
        dto.Iban,
        dto.Bic,
        BeneficiaryStatus.Active);
}
