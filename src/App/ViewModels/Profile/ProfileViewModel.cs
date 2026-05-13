using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using EcoBank.App.Services;
using EcoBank.App.ViewModels.Auth;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Domain.Users;
using EcoBank.Core.UseCases.Documents;

namespace EcoBank.App.ViewModels.Profile;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly INavigationService _navigation;
    private readonly GetUserDocumentsUseCase _getUserDocuments;
    private readonly GetUserDocumentContentUseCase _getUserDocumentContent;

    public User? CurrentUser => _userContext.SelectedUser;
    public string? UserDisplayName => CurrentUser is { } u
        ? $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.AppUserId
        : "Utilisateur";

    public string ProfileEmail => string.IsNullOrWhiteSpace(CurrentUser?.Email)
        ? "Non renseigné"
        : CurrentUser.Email;

    [ObservableProperty] private string _selectedLanguage = "fr";
    [ObservableProperty] private string _documentMessage = "Sélectionnez un document pour le charger.";
    public ObservableCollection<UserDocument> Documents { get; } = [];
    public bool HasDocuments => Documents.Any();
    public string DocumentsEmptyMessage => "Aucun document KYC/FATCA disponible pour ce profil.";

    public ProfileViewModel(
        UserContext userContext,
        INavigationService navigation,
        GetUserDocumentsUseCase getUserDocuments,
        GetUserDocumentContentUseCase getUserDocumentContent)
    {
        _userContext = userContext;
        _navigation = navigation;
        _getUserDocuments = getUserDocuments;
        _getUserDocumentContent = getUserDocumentContent;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            Documents.Clear();
            if (CurrentUser?.AppUserId is { Length: > 0 } appUserId)
            {
                foreach (var document in await _getUserDocuments.ExecuteAsync(appUserId, ct))
                {
                    Documents.Add(document);
                }
            }
            OnPropertyChanged(nameof(HasDocuments));
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les documents.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDocumentAsync(UserDocument document, CancellationToken ct)
    {
        if (CurrentUser?.AppUserId is not { Length: > 0 } appUserId)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var content = await _getUserDocumentContent.ExecuteAsync(appUserId, document, ct);
            DocumentMessage = content is null
                ? "Document indisponible."
                : $"{document.Name} chargé ({content.Content.Length:N0} octets).";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible d'ouvrir ce document.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ResetSession()
    {
        _userContext.Reset();
        _navigation.NavigateTo<LoginViewModel>();
    }
}
