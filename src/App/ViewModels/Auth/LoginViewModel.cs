using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Auth;
using EcoBank.Core.UseCases.Users;

namespace EcoBank.App.ViewModels.Auth;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthenticateUseCase _authenticateUseCase;
    private readonly SelectUserUseCase _selectUserUseCase;
    private readonly INavigationService _navigation;
    private readonly ISecureStorage _secureStorage;

    [ObservableProperty]
    private string _clientId = string.Empty;

    [ObservableProperty]
    private string _clientSecret = string.Empty;

    [ObservableProperty]
    private string _appUserId = string.Empty;

    [ObservableProperty]
    private bool _rememberCredentials;

    public LoginViewModel(
        AuthenticateUseCase authenticateUseCase,
        SelectUserUseCase selectUserUseCase,
        INavigationService navigation,
        ISecureStorage secureStorage)
    {
        _authenticateUseCase = authenticateUseCase;
        _selectUserUseCase = selectUserUseCase;
        _navigation = navigation;
        _secureStorage = secureStorage;
        _ = TryLoadSavedCredentialsAsync();
    }

    private async Task TryLoadSavedCredentialsAsync()
    {
        try
        {
            var savedId = await _secureStorage.LoadAsync("clientId");
            if (!string.IsNullOrEmpty(savedId))
            {
                ClientId = savedId;
                RememberCredentials = true;
                var savedAppUserId = await _secureStorage.LoadAsync("appUserId");
                if (!string.IsNullOrEmpty(savedAppUserId))
                    AppUserId = savedAppUserId;
            }
        }
        catch { /* ignore */ }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync(CancellationToken ct)
    {
        ClearError();
        IsBusy = true;
        try
        {
            var credentials = new Credentials(ClientId.Trim(), ClientSecret);
            await _authenticateUseCase.ExecuteAsync(credentials, ct);

            if (RememberCredentials)
            {
                await _secureStorage.SaveAsync("clientId", ClientId.Trim(), ct);
                await _secureStorage.SaveAsync("appUserId", AppUserId.Trim(), ct);
            }
            else
            {
                await _secureStorage.DeleteAsync("clientId", ct);
                await _secureStorage.DeleteAsync("appUserId", ct);
            }

            _selectUserUseCase.Execute(new User(AppUserId.Trim(), null, null, null, KycStatus.Unknown, null));
            _navigation.NavigateTo<MainShellViewModel>();
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode is 401 or 403)
        {
            ErrorMessage = "Identifiants invalides. Vérifiez votre Client ID et Client Secret.";
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode is 429)
        {
            ErrorMessage = "Trop de tentatives. Veuillez patienter avant de réessayer.";
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode is 503)
        {
            ErrorMessage = "Service temporairement indisponible (maintenance). Réessayez plus tard.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Erreur réseau. Vérifiez votre connexion internet.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "La requête a expiré. Vérifiez votre connexion.";
        }
        catch (Exception)
        {
            ErrorMessage = "Une erreur inattendue s'est produite.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        !string.IsNullOrWhiteSpace(AppUserId);

    partial void OnClientIdChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnClientSecretChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnAppUserIdChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
}
