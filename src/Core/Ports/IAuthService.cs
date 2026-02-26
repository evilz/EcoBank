using EcoBank.Core.Domain.Auth;

namespace EcoBank.Core.Ports;

public interface IAuthService
{
    Task<TokenResponse> AuthenticateAsync(Credentials credentials, CancellationToken ct = default);
}
