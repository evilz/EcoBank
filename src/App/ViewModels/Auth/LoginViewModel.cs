using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.UseCases.Auth;
using EcoBank.Core.UseCases.Users;

namespace EcoBank.App.ViewModels.Auth;

public enum LoginState
{
    ProfileSelection,
    PinEntry,
    AddProfile
}

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthenticateUseCase _authenticateUseCase;
    private readonly GetUserUseCase _getUserUseCase;
    private readonly SelectUserUseCase _selectUserUseCase;
    private readonly INavigationService _navigation;
    private readonly ProfileService _profileService;

    [ObservableProperty]
    private LoginState _currentState = LoginState.ProfileSelection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfiles))]
    private ObservableCollection<SavedProfile> _profiles = new();

    public bool HasProfiles => Profiles.Any();

    [ObservableProperty]
    private SavedProfile? _selectedProfile;

    [ObservableProperty]
    private string _pin = string.Empty;

    [ObservableProperty]
    private string _newPin = string.Empty;

    [ObservableProperty]
    private string _clientId = string.Empty;

    [ObservableProperty]
    private string _clientSecret = string.Empty;

    [ObservableProperty]
    private string _appUserId = string.Empty;

    [ObservableProperty]
    private bool _rememberCredentials = true;

    [ObservableProperty]
    private bool _isLoadingProfiles;

    public LoginViewModel(
        AuthenticateUseCase authenticateUseCase,
        GetUserUseCase getUserUseCase,
        SelectUserUseCase selectUserUseCase,
        INavigationService navigation,
        ProfileService profileService)
    {
        _authenticateUseCase = authenticateUseCase;
        _getUserUseCase = getUserUseCase;
        _selectUserUseCase = selectUserUseCase;
        _navigation = navigation;
        _profileService = profileService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoadingProfiles = true;
        try
        {
            var savedProfiles = await _profileService.GetProfilesAsync();
            Profiles = new ObservableCollection<SavedProfile>(savedProfiles);

            if (Profiles.Any())
            {
                CurrentState = LoginState.ProfileSelection;
            }
            else
            {
                CurrentState = LoginState.AddProfile;
            }
        }
        finally
        {
            IsLoadingProfiles = false;
        }
    }

    [RelayCommand]
    private void SelectProfile(SavedProfile? profile)
    {
        if (profile == null) return;
        SelectedProfile = profile;
        Pin = string.Empty;
        ClearError();
        CurrentState = LoginState.PinEntry;
    }

    [RelayCommand]
    private void NavigateToAddProfile()
    {
        ClearError();
        ClientId = string.Empty;
        ClientSecret = string.Empty;
        AppUserId = string.Empty;
        NewPin = string.Empty;
        CurrentState = LoginState.AddProfile;
    }

    [RelayCommand]
    private void CancelAddProfile()
    {
        ClearError();
        if (Profiles.Any())
        {
            CurrentState = LoginState.ProfileSelection;
        }
    }

    [RelayCommand]
    private void CancelPinEntry()
    {
        ClearError();
        Pin = string.Empty;
        SelectedProfile = null;
        CurrentState = LoginState.ProfileSelection;
    }

    [RelayCommand(CanExecute = nameof(CanSubmitPin))]
    private async Task SubmitPinAsync(CancellationToken ct)
    {
        if (SelectedProfile is null) return;

        ClearError();
        IsBusy = true;
        SubmitPinCommand.NotifyCanExecuteChanged();

        if (!_profileService.VerifyPin(Pin, SelectedProfile.PinHash))
        {
            ErrorMessage = "Code PIN incorrect.";
            IsBusy = false;
            SubmitPinCommand.NotifyCanExecuteChanged();
            return;
        }

        try
        {
            var secret = _profileService.DecryptSecret(SelectedProfile.EncryptedSecret, Pin);
            var credentials = new Credentials(SelectedProfile.ClientId, secret);
            await _authenticateUseCase.ExecuteAsync(credentials, ct);

            await NavigateToMainShellAsync(SelectedProfile.AppUserId, ct);
        }
        catch (Exception ex)
        {
            HandleLoginError(ex);
        }
        finally
        {
            IsBusy = false;
            SubmitPinCommand.NotifyCanExecuteChanged();
        }
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

            var appUserId = AppUserId.Trim();
            var clientId = ClientId.Trim();

            // Try fetch user to get their actual name to save
            var user = await _getUserUseCase.ExecuteAsync(appUserId, ct)
                ?? new User(appUserId, null, null, null, KycStatus.Unknown, null);

            var profileId = Guid.NewGuid().ToString();

            if (RememberCredentials && !string.IsNullOrWhiteSpace(NewPin))
            {
                var encryptedSecret = _profileService.EncryptSecret(ClientSecret, NewPin);
                var pinHash = _profileService.HashPin(NewPin);

                var newProfile = new SavedProfile(
                    Id: profileId,
                    ClientId: clientId,
                    AppUserId: appUserId,
                    EncryptedSecret: encryptedSecret,
                    PinHash: pinHash,
                    FirstName: user.FirstName,
                    LastName: user.LastName
                );

                await _profileService.SaveProfileAsync(newProfile, ct);
                
                // Refresh list
                Profiles = new ObservableCollection<SavedProfile>(await _profileService.GetProfilesAsync(ct));
            }

            _selectUserUseCase.Execute(user);
            _navigation.NavigateTo<MainShellViewModel>();
        }
        catch (Exception ex)
        {
            HandleLoginError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToMainShellAsync(string appUserId, CancellationToken ct)
    {
        var user = await _getUserUseCase.ExecuteAsync(appUserId, ct)
            ?? new User(appUserId, null, null, null, KycStatus.Unknown, null);
        _selectUserUseCase.Execute(user);
        _navigation.NavigateTo<MainShellViewModel>();
    }

    private void HandleLoginError(Exception ex)
    {
        switch (ex)
        {
            case HttpRequestException httpEx when ((int?)httpEx.StatusCode is 401 or 403):
                ErrorMessage = "Identifiants invalides. Vérifiez votre Client ID et Client Secret.";
                break;
            case HttpRequestException httpEx when ((int?)httpEx.StatusCode is 429):
                ErrorMessage = "Trop de tentatives. Veuillez patienter avant de réessayer.";
                break;
            case HttpRequestException httpEx when ((int?)httpEx.StatusCode is 503):
                ErrorMessage = "Service temporairement indisponible (maintenance). Réessayez plus tard.";
                break;
            case HttpRequestException:
                ErrorMessage = "Erreur réseau. Vérifiez votre connexion internet.";
                break;
            case TaskCanceledException:
                ErrorMessage = "La requête a expiré. Vérifiez votre connexion.";
                break;
            default:
                ErrorMessage = "Une erreur inattendue s'est produite.";
                break;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        !string.IsNullOrWhiteSpace(AppUserId) &&
        (!RememberCredentials || (RememberCredentials && !string.IsNullOrWhiteSpace(NewPin) && NewPin.Length >= 4));

    private bool CanSubmitPin() => !IsBusy && !string.IsNullOrWhiteSpace(Pin) && Pin.Length == 4;

    public bool IsProfileSelectionState => CurrentState == LoginState.ProfileSelection;
    public bool IsPinEntryState => CurrentState == LoginState.PinEntry;
    public bool IsAddProfileState => CurrentState == LoginState.AddProfile;

    partial void OnCurrentStateChanged(LoginState value)
    {
        OnPropertyChanged(nameof(IsProfileSelectionState));
        OnPropertyChanged(nameof(IsPinEntryState));
        OnPropertyChanged(nameof(IsAddProfileState));
    }

    partial void OnClientIdChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnClientSecretChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnAppUserIdChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnNewPinChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPinChanged(string value) => SubmitPinCommand.NotifyCanExecuteChanged();
}
