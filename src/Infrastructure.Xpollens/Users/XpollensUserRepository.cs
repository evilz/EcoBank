using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Users;

/// <summary>
/// Xpollens user endpoints.
/// GET api/v2.0/users — list users
/// GET api/v2.0/users/{appUserId} — get a specific user's info
/// </summary>
internal sealed record UserDto(
    [property: JsonPropertyName("appUserId")] string AppUserId,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("kycStatus")] string? KycStatus,
    [property: JsonPropertyName("lastLogin")] DateTimeOffset? LastLogin);

public sealed class XpollensUserRepository(HttpClient httpClient, ILogger<XpollensUserRepository> logger) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetUsersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        var query = $"api/v2.0/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        logger.LogDebug("Fetching users: page={Page}, pageSize={PageSize}", page, pageSize);

        var dtos = await httpClient.GetFromJsonAsync<List<UserDto>>(query, ct) ?? [];

        return dtos.Select(MapUser).ToList().AsReadOnly();
    }

    public async Task<User?> GetUserAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching user {AppUserId}", appUserId);
        var dto = await httpClient.GetFromJsonAsync<UserDto>($"api/v2.0/users/{appUserId}", ct);
        return dto is null ? null : MapUser(dto);
    }

    private static User MapUser(UserDto dto) => new(
        dto.AppUserId,
        dto.FirstName,
        dto.LastName,
        dto.Email,
        ParseKycStatus(dto.KycStatus),
        dto.LastLogin);

    private static KycStatus ParseKycStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "pending" => KycStatus.Pending,
        "inprogress" or "in_progress" => KycStatus.InProgress,
        "validated" or "approved" => KycStatus.Validated,
        "refused" or "rejected" => KycStatus.Refused,
        _ => KycStatus.Unknown
    };
}
