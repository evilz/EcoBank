using EcoBank.App.ViewModels.Contact;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Payments;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public sealed class ContactViewModelTests
{
    [Fact]
    public void Defaults_to_standard_payment_mode()
    {
        var vm = CreateViewModel();

        Assert.NotNull(vm.SelectedPaymentMode);
        Assert.Equal(PaymentExecutionMode.Standard, vm.SelectedPaymentMode!.Mode);
    }

    [Fact]
    public void Validation_message_tracks_missing_fields()
    {
        var vm = CreateViewModel();

        Assert.Equal("Sélectionnez un compte source.", vm.PaymentValidationMessage);

        vm.SelectedAccount = new Account("acc-1", "Compte principal", AccountType.Current, 100m, "EUR", "FR761234", AccountStatus.Active);
        Assert.Equal("Sélectionnez un bénéficiaire.", vm.PaymentValidationMessage);

        vm.SelectedBeneficiary = new Beneficiary("ben-1", "Alice", "FR761111", null, BeneficiaryStatus.Active);
        Assert.Equal("Le montant doit être supérieur à 0.", vm.PaymentValidationMessage);

        vm.Amount = 50m;
        Assert.Equal(string.Empty, vm.PaymentValidationMessage);
        Assert.True(vm.CanSubmitPayment);
    }

    private static ContactViewModel CreateViewModel()
    {
        var userContext = new UserContext();
        userContext.SelectUser(new User("user-1", "Test", "User", null, KycStatus.Validated, DateTimeOffset.UtcNow));

        var getAccounts = new GetAccountsUseCase(new FakeAccountRepository(), userContext);
        var getBeneficiaries = new GetBeneficiariesUseCase(userContext, new FakeBeneficiaryRepository());
        var getMandates = new GetMandatesUseCase(userContext, new FakePaymentRepository());
        var createSepaTransfer = new CreateSepaTransferUseCase(new FakePaymentRepository());

        return new ContactViewModel(
            userContext,
            getAccounts,
            getBeneficiaries,
            getMandates,
            createSepaTransfer);
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        public Task<IReadOnlyList<Account>> GetAccountsAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Account>>([]);

        public Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default) =>
            Task.FromResult<Account?>(null);
    }

    private sealed class FakeBeneficiaryRepository : IBeneficiaryRepository
    {
        public Task<IReadOnlyList<Beneficiary>> GetBeneficiariesAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Beneficiary>>([]);

        public Task<Beneficiary?> GetBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default) =>
            Task.FromResult<Beneficiary?>(null);

        public Task<Beneficiary> SaveBeneficiaryAsync(string appUserId, Beneficiary beneficiary, CancellationToken ct = default) =>
            Task.FromResult(beneficiary);

        public Task DeleteBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        public Task<PaymentResult> CreateSepaTransferAsync(PaymentOrder order, CancellationToken ct = default) =>
            Task.FromResult(new PaymentResult("p-1", PaymentStatus.Pending, null));

        public Task<PaymentResult> CreateInstantPaymentAsync(PaymentOrder order, CancellationToken ct = default) =>
            Task.FromResult(new PaymentResult("p-2", PaymentStatus.Pending, null));

        public Task<IReadOnlyList<PaymentResult>> GetPaymentStatusAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PaymentResult>>([]);

        public Task<IReadOnlyList<Mandate>> GetMandatesAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Mandate>>([]);
    }
}
