using EcoBank.App.Services;
using EcoBank.App.ViewModels;
using EcoBank.App.ViewModels.Auth;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Auth;
using EcoBank.Core.UseCases.Users;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public class LoginViewModelTests
{
    [Fact]
    public void LoginCommand_IsDisabled_WhenRequiredFieldsAreMissing()
    {
        var vm = CreateViewModel();

        Assert.False(vm.LoginCommand.CanExecute(null));

        vm.ClientId = "client";
        vm.ClientSecret = "secret";
        vm.AppUserId = "";

        Assert.False(vm.LoginCommand.CanExecute(null));

        vm.AppUserId = "user-1";
        // RememberCredentials defaults to true so PIN is required
        vm.NewPin = "1234";

        Assert.True(vm.LoginCommand.CanExecute(null));
    }

    [Fact]
    public async Task LoginCommand_NavigatesToMainShell_AfterSuccessfulAuthentication()
    {
        var navigation = new FakeNavigationService();
        var vm = CreateViewModel(navigationService: navigation);
        vm.ClientId = "client";
        vm.ClientSecret = "secret";
        vm.AppUserId = "user-1";
        vm.NewPin = "1234";

        await vm.LoginCommand.ExecuteAsync(null);

        Assert.Equal(typeof(MainShellViewModel), navigation.LastNavigatedType);
    }

    private static LoginViewModel CreateViewModel(
        INavigationService? navigationService = null,
        ProfileService? profileService = null)
    {
        var context = new UserContext();
        var authUseCase = new AuthenticateUseCase(new FakeAuthService(), context);
        var getUserUseCase = new GetUserUseCase(new FakeUserRepository());
        var selectUserUseCase = new SelectUserUseCase(context);

        return new LoginViewModel(
            authUseCase,
            getUserUseCase,
            selectUserUseCase,
            navigationService ?? new FakeNavigationService(),
            profileService ?? new ProfileService(new FakeSecureStorage()));
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<TokenResponse> AuthenticateAsync(Credentials credentials, CancellationToken ct = default)
            => Task.FromResult(new TokenResponse("token", "Bearer", 3600, DateTimeOffset.UtcNow.AddHours(1)));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<IReadOnlyList<User>> GetUsersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>());

        public Task<User?> GetUserAsync(string appUserId, CancellationToken ct = default)
            => Task.FromResult<User?>(new User(appUserId, "Test", "User", null, KycStatus.Validated, DateTimeOffset.UtcNow));
    }

    private sealed class FakeSecureStorage : ISecureStorage
    {
        public Task SaveAsync(string key, string value, CancellationToken ct = default) => Task.CompletedTask;
        public Task<string?> LoadAsync(string key, CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task DeleteAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public ViewModelBase CurrentPage { get; private set; } = null!;
        public event EventHandler<ViewModelBase>? PageChanged;
        public bool CanGoBack => false;
        public Type? LastNavigatedType { get; private set; }

        public void NavigateTo(ViewModelBase page)
        {
            CurrentPage = page;
            LastNavigatedType = page.GetType();
            PageChanged?.Invoke(this, page);
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
            LastNavigatedType = typeof(T);
        }

        public void GoBack() { }
    }
}
