using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Users;

public class GetUsersUseCase(IUserRepository userRepository)
{
    public Task<IReadOnlyList<User>> ExecuteAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken ct = default)
        => userRepository.GetUsersAsync(page, pageSize, search, ct);
}
