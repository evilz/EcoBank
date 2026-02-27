namespace EcoBank.Core.Domain.Auth;

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTimeOffset ExpiresAt);
