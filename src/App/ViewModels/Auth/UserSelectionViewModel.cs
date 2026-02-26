using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.UseCases.Users;

namespace EcoBank.App.ViewModels.Auth;

public partial class UserSelectionViewModel : ViewModelBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly SelectUserUseCase _selectUserUseCase;
    private readonly INavigationService _navigation;

    private int _currentPage = 1;
    private bool _hasMorePages = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _isEmpty;

    public ObservableCollection<User> Users { get; } = [];

    public UserSelectionViewModel(
        GetUsersUseCase getUsersUseCase,
        SelectUserUseCase selectUserUseCase,
        INavigationService navigation)
    {
        _getUsersUseCase = getUsersUseCase;
        _selectUserUseCase = selectUserUseCase;
        _navigation = navigation;
        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync(bool reset = true)
    {
        if (IsBusy) return;
        ClearError();
        IsBusy = true;

        try
        {
            if (reset)
            {
                Users.Clear();
                _currentPage = 1;
                _hasMorePages = true;
            }

            var results = await _getUsersUseCase.ExecuteAsync(_currentPage, 20, SearchQuery.Trim().NullIfEmpty());

            foreach (var user in results)
                Users.Add(user);

            _hasMorePages = results.Count == 20;
            IsEmpty = Users.Count == 0;
            _currentPage++;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les utilisateurs.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadUsersAsync(reset: true);
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (!_hasMorePages || IsLoadingMore) return;
        IsLoadingMore = true;
        try
        {
            await LoadUsersAsync(reset: false);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    private void SelectUser(User user)
    {
        _selectUserUseCase.Execute(user);
        _navigation.NavigateTo<MainShellViewModel>();
    }

    [RelayCommand]
    private void ChangeCredentials()
    {
        _navigation.NavigateTo<LoginViewModel>();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = LoadUsersAsync(reset: true);
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
