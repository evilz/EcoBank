using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Domain.Security;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Documents;
using EcoBank.Core.UseCases.Payments;
using EcoBank.Core.UseCases.Security;
using Xunit;

namespace EcoBank.App.Tests.UseCases;

public sealed class ClientBankingUseCaseTests
{
    [Fact]
    public async Task CreateSepaTransfer_rejects_non_positive_amount()
    {
        var useCase = new CreateSepaTransferUseCase(new CapturingPaymentRepository());
        var order = new PaymentOrder("account-1", "beneficiary-1", 0m, "EUR", "Test", PaymentExecutionMode.Standard);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(order, TestContext.Current.CancellationToken));

        Assert.Contains("amount", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUserDocuments_returns_empty_collection_when_no_document_keys_are_known()
    {
        var useCase = new GetUserDocumentsUseCase(new EmptyDocumentRepository());

        var documents = await useCase.ExecuteAsync("user-1", TestContext.Current.CancellationToken);

        Assert.Empty(documents);
    }

    [Fact]
    public async Task RequestStrongAuthentication_forwards_action_context()
    {
        var repository = new CapturingSecurityRepository();
        var useCase = new RequestStrongAuthenticationUseCase(repository);

        await useCase.ExecuteAsync(
            new StrongAuthenticationRequest("user-1", "DisplayPin", "card-1", 123.45m, "EUR"),
            TestContext.Current.CancellationToken);

        Assert.Equal("DisplayPin", repository.LastRequest?.Action);
        Assert.Equal("card-1", repository.LastRequest?.ResourceId);
    }

    private sealed class CapturingPaymentRepository : IPaymentRepository
    {
        public Task<PaymentResult> CreateSepaTransferAsync(PaymentOrder order, CancellationToken ct = default) =>
            Task.FromResult(new PaymentResult("payment-1", PaymentStatus.Pending, "En cours"));

        public Task<PaymentResult> CreateInstantPaymentAsync(PaymentOrder order, CancellationToken ct = default) =>
            Task.FromResult(new PaymentResult("payment-1", PaymentStatus.Pending, "En cours"));

        public Task<IReadOnlyList<PaymentResult>> GetPaymentStatusAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PaymentResult>>([]);

        public Task<IReadOnlyList<Mandate>> GetMandatesAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Mandate>>([]);
    }

    private sealed class EmptyDocumentRepository : IDocumentRepository
    {
        public Task<IReadOnlyList<UserDocument>> GetDocumentsAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UserDocument>>([]);

        public Task<UserDocumentContent?> GetDocumentContentAsync(string appUserId, string key, DocumentKind kind, CancellationToken ct = default) =>
            Task.FromResult<UserDocumentContent?>(null);
    }

    private sealed class CapturingSecurityRepository : ISecurityRepository
    {
        public StrongAuthenticationRequest? LastRequest { get; private set; }

        public Task<StrongAuthenticationResult> RequestStrongAuthenticationAsync(StrongAuthenticationRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult(new StrongAuthenticationResult("sca-1", StrongAuthenticationStatus.Pending, "Validation requise"));
        }
    }
}
