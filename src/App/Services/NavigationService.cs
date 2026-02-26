using EcoBank.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EcoBank.App.Services;

public sealed class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly Stack<ViewModelBase> _history = new();
    private ViewModelBase _currentPage = null!;

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set
        {
            _currentPage = value;
            PageChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<ViewModelBase>? PageChanged;

    public bool CanGoBack => _history.Count > 0;

    public void NavigateTo(ViewModelBase page)
    {
        if (_currentPage is not null)
            _history.Push(_currentPage);
        CurrentPage = page;
    }

    public void NavigateTo<T>() where T : ViewModelBase
    {
        var page = serviceProvider.GetRequiredService<T>();
        NavigateTo(page);
    }

    public void GoBack()
    {
        if (_history.TryPop(out var page))
            CurrentPage = page;
    }
}
