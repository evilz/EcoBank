using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Auth;

internal sealed record TokenResponseDto(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

/// <summary>
/// Authenticates against the Xpollens sandbox identity server using the
/// OAuth2 client_credentials flow.
/// POST https://sb-connect.xpollens.com/connect/token
/// Content-Type: application/x-www-form-urlencoded
/// </summary>
public sealed class XpollensAuthService(HttpClient httpClient, ILogger<XpollensAuthService> logger) : IAuthService
{
    private const string TokenEndpoint = "connect/token";

    public async Task<TokenResponse> AuthenticateAsync(Credentials credentials, CancellationToken ct = default)
    {
        logger.LogInformation("Authenticating with Xpollens (client_id={ClientId})", credentials.ClientId);

        var formContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", credentials.ClientId),
            new KeyValuePair<string, string>("client_secret", credentials.ClientSecret),
            new KeyValuePair<string, string>("scope", "partner"),
        ]);

        using var response = await httpClient.PostAsync(TokenEndpoint, formContent, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Authentication failed: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Authentification échouée : {response.StatusCode}", null, response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<TokenResponseDto>(ct)
            ?? throw new InvalidOperationException("Réponse token vide.");

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(dto.ExpiresIn);
        logger.LogInformation("Authentication successful, token expires at {ExpiresAt}", expiresAt);

        return new TokenResponse(dto.AccessToken, dto.TokenType, dto.ExpiresIn, expiresAt);
    }
}
