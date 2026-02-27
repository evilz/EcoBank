using EcoBank.App.ViewModels;

namespace EcoBank.App.Services;

public interface INavigationService
{
    ViewModelBase CurrentPage { get; }
    event EventHandler<ViewModelBase>? PageChanged;
    void NavigateTo(ViewModelBase page);
    void NavigateTo<T>() where T : ViewModelBase;
    bool CanGoBack { get; }
    void GoBack();
}
