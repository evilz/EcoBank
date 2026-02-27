using CommunityToolkit.Mvvm.ComponentModel;
using EcoBank.App.Services;

namespace EcoBank.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public MainWindowViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.PageChanged += (_, vm) => CurrentPage = vm;
        CurrentPage = navigation.CurrentPage;
    }
}
