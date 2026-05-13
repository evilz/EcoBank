using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Security;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Security;

internal sealed record StrongAuthenticationRequestDto(
    [property: JsonPropertyName("appUserId")] string AppUserId,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("resourceId")] string? ResourceId,
    [property: JsonPropertyName("amount")] decimal? Amount,
    [property: JsonPropertyName("currency")] string? Currency);

internal sealed record StrongAuthenticationResponseDto(
    [property: JsonPropertyName("requestId")] string? RequestId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("message")] string? Message);

public sealed class XpollensSecurityRepository(HttpClient httpClient, ILogger<XpollensSecurityRepository> logger) : ISecurityRepository
{
    public async Task<StrongAuthenticationResult> RequestStrongAuthenticationAsync(StrongAuthenticationRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Requesting SCA for {Action}", request.Action);
        var dto = new StrongAuthenticationRequestDto(request.AppUserId, request.Action, request.ResourceId, request.Amount, request.Currency);
        var response = await httpClient.PostAsJsonAsync("api/v2.0/strong-authentication-request", dto, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<StrongAuthenticationResponseDto>(cancellationToken: ct);
        return new StrongAuthenticationResult(
            result?.RequestId ?? "",
            ParseStatus(result?.Status),
            result?.Message);
    }

    private static StrongAuthenticationStatus ParseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "approved" or "validated" => StrongAuthenticationStatus.Approved,
        "rejected" => StrongAuthenticationStatus.Rejected,
        "expired" => StrongAuthenticationStatus.Expired,
        "pending" or "created" => StrongAuthenticationStatus.Pending,
        _ => StrongAuthenticationStatus.Unknown
    };
}
