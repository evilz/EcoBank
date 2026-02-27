using EcoBank.Core.Application;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Auth;

public class AuthenticateUseCase(IAuthService authService, UserContext userContext)
{
    public async Task<TokenResponse> ExecuteAsync(Credentials credentials, CancellationToken ct = default)
    {
        var token = await authService.AuthenticateAsync(credentials, ct);
        userContext.SetCredentials(credentials);
        userContext.SetToken(token);
        return token;
    }
}
