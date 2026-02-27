using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.App.ViewModels.Auth;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Users;

namespace EcoBank.App.ViewModels.Profile;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly INavigationService _navigation;

    public User? CurrentUser => _userContext.SelectedUser;
    public string? UserDisplayName => CurrentUser is { } u
        ? $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.AppUserId
        : null;

    [ObservableProperty] private bool _darkModeEnabled;
    [ObservableProperty] private string _selectedLanguage = "fr";

    public ProfileViewModel(UserContext userContext, INavigationService navigation)
    {
        _userContext = userContext;
        _navigation = navigation;
    }

    [RelayCommand]
    private void ResetSession()
    {
        _userContext.Reset();
        _navigation.NavigateTo<LoginViewModel>();
    }
}
