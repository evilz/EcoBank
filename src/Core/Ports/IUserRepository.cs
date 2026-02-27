using EcoBank.Core.Domain.Users;

namespace EcoBank.Core.Ports;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetUsersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default);
    Task<User?> GetUserAsync(string appUserId, CancellationToken ct = default);
}
