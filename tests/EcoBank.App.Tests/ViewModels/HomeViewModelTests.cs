using EcoBank.App.Services;
using EcoBank.App.ViewModels.Home;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Operations;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public sealed class HomeViewModelTests
{
    [Fact]
    public void GreetingName_does_not_fall_back_to_user_id_when_first_name_is_missing()
    {
        var userContext = new UserContext();
        userContext.SelectUser(new User("app-user-42", null, "Durand", null, KycStatus.Validated, null));

        var vm = new HomeViewModel(
            userContext,
            new GetAccountsUseCase(new EmptyAccountRepository(), userContext),
            new GetOperationsUseCase(new EmptyOperationRepository()),
            new ShellNavigationContext());

        Assert.Equal(string.Empty, vm.GreetingName);
        Assert.Equal("Bonjour", vm.WelcomeMessage);
    }

    private sealed class EmptyAccountRepository : IAccountRepository
    {
        public Task<IReadOnlyList<Account>> GetAccountsAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Account>>([]);

        public Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default) =>
            Task.FromResult<Account?>(null);
    }

    private sealed class EmptyOperationRepository : IOperationRepository
    {
        public Task<IReadOnlyList<Operation>> GetOperationsAsync(
            string? accountId = null,
            OperationType? type = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            string? search = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Operation>>([]);

        public Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default) =>
            Task.FromResult<Operation?>(null);
    }
}
