using EcoBank.Core.Application;
using EcoBank.Core.Domain.Users;

namespace EcoBank.Core.UseCases.Users;

public class SelectUserUseCase(UserContext userContext)
{
    public void Execute(User user) => userContext.SelectUser(user);
}
