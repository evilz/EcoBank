using EcoBank.Core.Domain.Security;

namespace EcoBank.Core.Ports;

public interface ISecurityRepository
{
    Task<StrongAuthenticationResult> RequestStrongAuthenticationAsync(StrongAuthenticationRequest request, CancellationToken ct = default);
}
