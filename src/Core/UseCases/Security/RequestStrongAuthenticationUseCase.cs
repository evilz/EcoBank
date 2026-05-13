using EcoBank.Core.Domain.Security;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Security;

public sealed class RequestStrongAuthenticationUseCase(ISecurityRepository securityRepository)
{
    public Task<StrongAuthenticationResult> ExecuteAsync(StrongAuthenticationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.AppUserId))
        {
            throw new ArgumentException("App user id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            throw new ArgumentException("Authentication action is required.", nameof(request));
        }

        return securityRepository.RequestStrongAuthenticationAsync(request, ct);
    }
}
