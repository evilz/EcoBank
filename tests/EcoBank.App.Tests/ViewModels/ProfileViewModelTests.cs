using EcoBank.App.Services;
using EcoBank.App.ViewModels;
using EcoBank.App.ViewModels.Profile;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Documents;
using Avalonia.Headless.XUnit;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public sealed class ProfileViewModelTests
{
    [AvaloniaFact]
    public async Task OpenDocument_displays_image_inside_app_when_mime_type_is_missing_but_extension_is_image()
    {
        var repository = new SingleDocumentRepository(
            new UserDocumentContent("id-card", "fd04309b-9d04-4071-b38d-86a1e7dedf5d", DocumentKind.Kyc, null, TinyPng));
        var vm = CreateViewModel(repository);
        var document = new UserDocument("id-card", "Specimen Carte identite Recto.png", DocumentKind.Kyc, null, null);

        await vm.OpenDocumentCommand.ExecuteAsync(document);

        Assert.True(vm.IsDocumentImage);
        Assert.NotNull(vm.DocumentImageBitmap);
        Assert.Null(vm.DocumentFilePath);
        Assert.Equal("Specimen Carte identite Recto.png", vm.ViewingDocumentName);
    }

    [Fact]
    public async Task OpenDocument_preserves_original_extension_for_downloaded_file_preview()
    {
        var repository = new SingleDocumentRepository(
            new UserDocumentContent("contract", "contrat.final.txt", DocumentKind.Kyc, "text/plain", [1, 2, 3]));
        var vm = CreateViewModel(repository);
        var document = new UserDocument("contract", "contrat.final.txt", DocumentKind.Kyc, "text/plain", null);

        await vm.OpenDocumentCommand.ExecuteAsync(document);

        Assert.False(vm.IsDocumentImage);
        Assert.NotNull(vm.DocumentFilePath);
        Assert.EndsWith(".txt", vm.DocumentFilePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenDocument_handles_missing_content_and_document_names()
    {
        var repository = new SingleDocumentRepository(
            new UserDocumentContent("unknown", null!, DocumentKind.Kyc, null, [1, 2, 3]));
        var vm = CreateViewModel(repository);
        var document = new UserDocument("unknown", null!, DocumentKind.Kyc, null, null);

        await vm.OpenDocumentCommand.ExecuteAsync(document);

        Assert.NotNull(vm.DocumentFilePath);
        Assert.EndsWith(".bin", vm.DocumentFilePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("document", vm.ViewingDocumentName);
    }

    private static ProfileViewModel CreateViewModel(IDocumentRepository repository)
    {
        var userContext = new UserContext();
        userContext.SelectUser(new User("user-1", "Alice", "Durand", null, KycStatus.Validated, null));

        return new ProfileViewModel(
            userContext,
            new FakeNavigationService(),
            new GetUserDocumentsUseCase(repository),
            new GetUserDocumentContentUseCase(repository),
            new ShellNavigationContext());
    }

    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=");

    private sealed class SingleDocumentRepository(UserDocumentContent content) : IDocumentRepository
    {
        public Task<IReadOnlyList<UserDocument>> GetDocumentsAsync(string appUserId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UserDocument>>([]);

        public Task<UserDocumentContent?> GetDocumentContentAsync(string appUserId, string key, DocumentKind kind, CancellationToken ct = default) =>
            Task.FromResult<UserDocumentContent?>(content);
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public ViewModelBase CurrentPage { get; private set; } = null!;
        public event EventHandler<ViewModelBase>? PageChanged;
        public bool CanGoBack => false;
        public void NavigateTo(ViewModelBase page)
        {
            CurrentPage = page;
            PageChanged?.Invoke(this, page);
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
        }

        public void GoBack()
        {
        }
    }
}
