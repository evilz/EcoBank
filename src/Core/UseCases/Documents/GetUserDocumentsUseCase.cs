using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Documents;

public sealed class GetUserDocumentsUseCase(IDocumentRepository documentRepository)
{
    public Task<IReadOnlyList<UserDocument>> ExecuteAsync(string appUserId, CancellationToken ct = default)
    {
        return string.IsNullOrWhiteSpace(appUserId)
            ? Task.FromResult<IReadOnlyList<UserDocument>>([])
            : documentRepository.GetDocumentsAsync(appUserId, ct);
    }
}
