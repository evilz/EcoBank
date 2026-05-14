using EcoBank.App.Services;
using EcoBank.App.ViewModels.Accounts;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Operations;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public sealed class AccountsViewModelTests
{
    [Fact]
    public async Task Load_fetches_operations_for_selected_account()
    {
        var userContext = new UserContext();
        userContext.SelectUser(new User("user-1", "Alice", null, null, KycStatus.Validated, null));
        var operationRepository = new CapturingOperationRepository();
        var accountRepository = new SingleAccountRepository();

        var vm = new AccountsViewModel(
            new GetAccountsUseCase(accountRepository, userContext),
            new GetVirtualIbansUseCase(new EmptyVirtualIbanRepository()),
            new ListBankStatementsUseCase(new EmptyBankStatementRepository(), userContext),
            new GetBankStatementUseCase(new EmptyBankStatementRepository()),
            new GetOperationsUseCase(operationRepository),
            new ShellNavigationContext());

        await vm.LoadCommand.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal("acc-1", operationRepository.LastAccountId);
        Assert.Single(vm.Operations);
        Assert.True(vm.HasOperations);
    }

    [Fact]
    public async Task Load_handles_null_operations_result()
    {
        var userContext = new UserContext();
        userContext.SelectUser(new User("user-1", "Alice", null, null, KycStatus.Validated, null));

        var vm = new AccountsViewModel(
            new GetAccountsUseCase(new SingleAccountRepository(), userContext),
            new GetVirtualIbansUseCase(new EmptyVirtualIbanRepository()),
            new ListBankStatementsUseCase(new EmptyBankStatementRepository(), userContext),
            new GetBankStatementUseCase(new EmptyBankStatementRepository()),
            new GetOperationsUseCase(new NullOperationRepository()),
            new ShellNavigationContext());

        await vm.LoadCommand.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Empty(vm.Operations);
        Assert.True(vm.HasNoOperations);
    }

    private sealed class SingleAccountRepository : IAccountRepository
    {
        public Task<IReadOnlyList<Account>> GetAccountsAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Account>>([
                new Account("acc-1", "Compte courant", AccountType.Current, 120m, "EUR", "FR761234", AccountStatus.Active)
            ]);

        public Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default) =>
            Task.FromResult<Account?>(null);
    }

    private sealed class EmptyVirtualIbanRepository : IVirtualIbanRepository
    {
        public Task<IReadOnlyList<VirtualIban>> GetVirtualIbansAsync(string accountId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VirtualIban>>([]);

        public Task<VirtualIban> CreateVirtualIbanAsync(string accountId, string? label, CancellationToken ct = default) =>
            Task.FromResult(new VirtualIban("viban-1", accountId, "FR760000", label, AccountStatus.Active));
    }

    private sealed class EmptyBankStatementRepository : IBankStatementRepository
    {
        public Task<IReadOnlyList<BankStatement>> ListBankStatementsAsync(string? accountId = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<BankStatement>>([]);

        public Task<BankStatement?> GetBankStatementAsync(string bankStatementId, CancellationToken ct = default) =>
            Task.FromResult<BankStatement?>(null);
    }

    private sealed class CapturingOperationRepository : IOperationRepository
    {
        public string? LastAccountId { get; private set; }

        public Task<IReadOnlyList<Operation>> GetOperationsAsync(
            string? accountId = null,
            OperationType? type = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            string? search = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            LastAccountId = accountId;
            return Task.FromResult<IReadOnlyList<Operation>>([
                new Operation("op-1", accountId ?? "", -12.30m, "EUR", "Paiement", OperationType.Debit, OperationStatus.Completed, DateTimeOffset.UtcNow, "Carte", null)
            ]);
        }

        public Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default) =>
            Task.FromResult<Operation?>(null);
    }

    private sealed class NullOperationRepository : IOperationRepository
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
            Task.FromResult<IReadOnlyList<Operation>>(null!);

        public Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default) =>
            Task.FromResult<Operation?>(null);
    }
}
