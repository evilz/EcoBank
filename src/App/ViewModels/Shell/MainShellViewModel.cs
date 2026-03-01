using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.ViewModels.Cards;
using EcoBank.App.ViewModels.Contact;
using EcoBank.App.ViewModels.Home;
using EcoBank.App.ViewModels.Operations;
using EcoBank.App.ViewModels.Profile;
using EcoBank.Core.Application;

namespace EcoBank.App.ViewModels.Shell;

public record NavTab(string Key, string Label, string Icon, ViewModelBase Content);

public partial class MainShellViewModel : ViewModelBase
{
    private readonly UserContext _userContext;

    public ObservableCollection<NavTab> Tabs { get; }

    [ObservableProperty]
    private NavTab _selectedTab;

    [ObservableProperty]
    private ViewModelBase _currentContent;

    public string UserDisplayName =>
        _userContext.SelectedUser is { } u
            ? $"{u.FirstName} {u.LastName}".Trim().NullIfEmpty() ?? u.AppUserId
            : "Utilisateur";

    public MainShellViewModel(
        UserContext userContext,
        HomeViewModel homeVm,
        CardsViewModel cardsVm,
        ContactViewModel contactVm,
        OperationsViewModel operationsVm,
        ProfileViewModel profileVm)
    {
        _userContext = userContext;

        Tabs =
        [
            new("home",     "Accueil",   "home",     homeVm),
            new("cards",    "Carte",     "card",     cardsVm),
            new("contact",  "Contact",   "contact",  contactVm),
            new("transfer", "Virement",  "transfer", operationsVm),
            new("menu",     "Menu",      "menu",     profileVm),
        ];

        _selectedTab = Tabs[0];
        _currentContent = Tabs[0].Content;
    }

    [RelayCommand]
    private void SelectTab(NavTab tab)
    {
        SelectedTab = tab;
        CurrentContent = tab.Content;
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
