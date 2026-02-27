using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Users;

/// <summary>
/// Xpollens user endpoints.
/// GET api/v2.0/users — list users (paginated envelope)
/// GET api/v2.0/users?AppUserId={appUserId} — filter by appUserId, returns paged envelope; first value is selected
/// </summary>
internal sealed record UserProfileDto(
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("email")] string? Email);

internal sealed record XpollensUserDto(
    [property: JsonPropertyName("appUserId")] string AppUserId,
    [property: JsonPropertyName("userRecordStatus")] string? UserRecordStatus,
    [property: JsonPropertyName("activationDate")] DateTimeOffset? ActivationDate,
    [property: JsonPropertyName("profile")] UserProfileDto? Profile);

internal sealed record UserPagedResponseDto(
    [property: JsonPropertyName("values")] List<XpollensUserDto>? Values);

public sealed class XpollensUserRepository(HttpClient httpClient, ILogger<XpollensUserRepository> logger) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetUsersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        var query = $"api/v2.0/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        logger.LogDebug("Fetching users: page={Page}, pageSize={PageSize}", page, pageSize);

        var paged = await httpClient.GetFromJsonAsync<UserPagedResponseDto>(query, ct);
        return (paged?.Values ?? []).Select(MapUser).ToList().AsReadOnly();
    }

    public async Task<User?> GetUserAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching user {AppUserId}", appUserId);
        var paged = await httpClient.GetFromJsonAsync<UserPagedResponseDto>(
            $"api/v2.0/users?AppUserId={Uri.EscapeDataString(appUserId)}", ct);
        var dto = paged?.Values?.FirstOrDefault();
        return dto is null ? null : MapUser(dto);
    }

    private static User MapUser(XpollensUserDto dto) => new(
        dto.AppUserId,
        dto.Profile?.FirstName,
        dto.Profile?.LastName,
        dto.Profile?.Email,
        ParseKycStatus(dto.UserRecordStatus),
        dto.ActivationDate);

    private static KycStatus ParseKycStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "pending" => KycStatus.Pending,
        "inprogress" or "in_progress" => KycStatus.InProgress,
        "validated" or "approved" => KycStatus.Validated,
        "refused" or "rejected" => KycStatus.Refused,
        _ => KycStatus.Unknown
    };
}
