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

public enum ProfileSection
{
    Main,
    PersonalInfo,
    Security,
    PaymentMethods,
    Notifications,
    Documents,
    Settings
}

public partial class ProfileViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly INavigationService _navigation;
    private readonly GetUserDocumentsUseCase _getUserDocuments;
    private readonly GetUserDocumentContentUseCase _getUserDocumentContent;
    private readonly ShellNavigationContext _shellNav;
    private string? _lastPdfTempPath;

    public User? CurrentUser => _userContext.SelectedUser;
    public string? UserDisplayName => CurrentUser is { } u
        ? $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.AppUserId
        : "Utilisateur";

    public string ProfileEmail => string.IsNullOrWhiteSpace(CurrentUser?.Email)
        ? "Non renseigné"
        : CurrentUser.Email;

    public string ProfileFirstName => string.IsNullOrWhiteSpace(CurrentUser?.FirstName) ? "Non renseigné" : CurrentUser.FirstName;
    public string ProfileLastName => string.IsNullOrWhiteSpace(CurrentUser?.LastName) ? "Non renseigné" : CurrentUser.LastName;
    public string ProfileKycStatus => FormatKycStatus(CurrentUser?.KycStatus);

    [ObservableProperty] private string _selectedLanguage = "fr";
    [ObservableProperty] private string _documentMessage = "Sélectionnez un document pour le charger.";
    [ObservableProperty] private ProfileSection _currentSection = ProfileSection.Main;

    // Document viewer state
    [ObservableProperty] private byte[]? _documentBytes;
    [ObservableProperty] private string? _documentMimeType;
    [ObservableProperty] private string? _viewingDocumentName;

    public bool IsDocumentImage => DocumentMimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsDocumentPdf => string.Equals(DocumentMimeType, "application/pdf", StringComparison.OrdinalIgnoreCase);
    public bool IsViewingDocument => DocumentBytes is not null || IsDocumentPdf;

    // Section visibility helpers
    public bool IsMainSection => CurrentSection == ProfileSection.Main;
    public bool IsPersonalInfoSection => CurrentSection == ProfileSection.PersonalInfo;
    public bool IsSecuritySection => CurrentSection == ProfileSection.Security;
    public bool IsPaymentMethodsSection => CurrentSection == ProfileSection.PaymentMethods;
    public bool IsNotificationsSection => CurrentSection == ProfileSection.Notifications;
    public bool IsDocumentsSection => CurrentSection == ProfileSection.Documents;
    public bool IsSettingsSection => CurrentSection == ProfileSection.Settings;

    // Notification toggles
    [ObservableProperty] private bool _notifPayments = true;
    [ObservableProperty] private bool _notifSecurity = true;
    [ObservableProperty] private bool _notifOffers = false;

    public ObservableCollection<UserDocument> Documents { get; } = [];
    public bool HasDocuments => Documents.Any();
    public string DocumentsEmptyMessage => "Aucun document KYC/FATCA disponible pour ce profil.";

    public ProfileViewModel(
        UserContext userContext,
        INavigationService navigation,
        GetUserDocumentsUseCase getUserDocuments,
        GetUserDocumentContentUseCase getUserDocumentContent,
        ShellNavigationContext shellNav)
    {
        _userContext = userContext;
        _navigation = navigation;
        _getUserDocuments = getUserDocuments;
        _getUserDocumentContent = getUserDocumentContent;
        _shellNav = shellNav;
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

    // Section navigation commands
    [RelayCommand] private void GoToPersonalInfo() => NavigateTo(ProfileSection.PersonalInfo);
    [RelayCommand] private void GoToSecurity() => NavigateTo(ProfileSection.Security);
    [RelayCommand] private void GoToPaymentMethods() => _shellNav.GoToCards();
    [RelayCommand] private void GoToNotifications() => NavigateTo(ProfileSection.Notifications);
    [RelayCommand] private void GoToDocuments()
    {
        NavigateTo(ProfileSection.Documents);
        _ = LoadAsync(CancellationToken.None);
    }
    [RelayCommand] private void GoToSettings() => NavigateTo(ProfileSection.Settings);
    [RelayCommand] private void GoBackToMain()
    {
        ClearDocumentPreview();
        CleanupTempPdfFile();
        CurrentSection = ProfileSection.Main;
        UpdateSectionProperties();
    }
    
    [RelayCommand]
    private void CloseDocumentPreview() => ClearDocumentPreview();

    private void NavigateTo(ProfileSection section)
    {
        CurrentSection = section;
        UpdateSectionProperties();
    }

    private void UpdateSectionProperties()
    {
        OnPropertyChanged(nameof(IsMainSection));
        OnPropertyChanged(nameof(IsPersonalInfoSection));
        OnPropertyChanged(nameof(IsSecuritySection));
        OnPropertyChanged(nameof(IsPaymentMethodsSection));
        OnPropertyChanged(nameof(IsNotificationsSection));
        OnPropertyChanged(nameof(IsDocumentsSection));
        OnPropertyChanged(nameof(IsSettingsSection));
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
        ClearDocumentPreview();
        try
        {
            var content = await _getUserDocumentContent.ExecuteAsync(appUserId, document, ct);
            if (content is null)
            {
                DocumentMessage = "Document indisponible.";
                return;
            }

            ViewingDocumentName = content.Name;
            DocumentMimeType = content.ContentType ?? document.ContentType;
            OnPropertyChanged(nameof(IsDocumentImage));
            OnPropertyChanged(nameof(IsDocumentPdf));

            if (IsDocumentImage)
            {
                DocumentBytes = content.Content;
                OnPropertyChanged(nameof(IsViewingDocument));
                DocumentMessage = $"{document.Name} chargé ({content.Content.Length:N0} octets).";
            }
            else if (IsDocumentPdf)
            {
                var safeName = string.Concat(
                    content.Name
                        .Select(c => Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c));
                var tempPath = Path.Combine(Path.GetTempPath(), $"ecobank_{safeName}_{Guid.NewGuid():N}.pdf");
                await File.WriteAllBytesAsync(tempPath, content.Content, ct);
                CleanupTempPdfFile();
                _lastPdfTempPath = tempPath;
                DocumentMessage = $"PDF ouvert dans le visualiseur système.";
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(tempPath) { UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
                catch
                {
                    DocumentMessage = $"PDF enregistré dans : {tempPath}";
                }
            }
            else
            {
                DocumentMessage = $"{document.Name} chargé ({content.Content.Length:N0} octets).";
            }
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
        CleanupTempPdfFile();
        ClearDocumentPreview();
        _userContext.Reset();
        _navigation.NavigateTo<LoginViewModel>();
    }
    
    private void ClearDocumentPreview()
    {
        DocumentBytes = null;
        DocumentMimeType = null;
        ViewingDocumentName = null;
        OnPropertyChanged(nameof(IsViewingDocument));
        OnPropertyChanged(nameof(IsDocumentImage));
        OnPropertyChanged(nameof(IsDocumentPdf));
    }

    private void CleanupTempPdfFile()
    {
        if (string.IsNullOrWhiteSpace(_lastPdfTempPath))
        {
            return;
        }

        try
        {
            if (File.Exists(_lastPdfTempPath))
            {
                File.Delete(_lastPdfTempPath);
            }
        }
        catch
        {
        }
        finally
        {
            _lastPdfTempPath = null;
        }
    }

    private static string FormatKycStatus(KycStatus? status) => status switch
    {
        KycStatus.Validated => "KYC validé",
        KycStatus.InProgress => "KYC en cours",
        KycStatus.Pending => "KYC en attente",
        KycStatus.Refused => "KYC refusé",
        _ => "KYC inconnu"
    };
}
