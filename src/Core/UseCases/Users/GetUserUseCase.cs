using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Users;

public class GetUserUseCase(IUserRepository userRepository)
{
    public Task<User?> ExecuteAsync(string appUserId, CancellationToken ct = default)
        => userRepository.GetUserAsync(appUserId, ct);
}
