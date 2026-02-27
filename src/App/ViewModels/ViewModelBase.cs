using CommunityToolkit.Mvvm.ComponentModel;

namespace EcoBank.App.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public void ClearError() => ErrorMessage = null;
}
