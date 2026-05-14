using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Media.Imaging;
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
    private const string TempDocumentPrefix = "ecobank_";
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
    [ObservableProperty] private Bitmap? _documentImageBitmap;
    [ObservableProperty] private string? _documentMimeType;
    [ObservableProperty] private string? _viewingDocumentName;
    [ObservableProperty] private string? _documentFilePath;

    public bool IsDocumentImage => DocumentMimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsDocumentPdf => string.Equals(DocumentMimeType, "application/pdf", StringComparison.OrdinalIgnoreCase);
    public bool IsDocumentFile => !string.IsNullOrWhiteSpace(DocumentFilePath);
    public bool IsViewingDocument => DocumentImageBitmap is not null || IsDocumentFile;

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

            var displayName = ResolveDisplayDocumentName(content.Name, document.Name);
            ViewingDocumentName = displayName;
            DocumentMimeType = ResolveDocumentMimeType(content.ContentType ?? document.ContentType, displayName);
            OnPropertyChanged(nameof(IsDocumentImage));
            OnPropertyChanged(nameof(IsDocumentPdf));

            if (IsDocumentImage)
            {
                using var stream = new MemoryStream(content.Content);
                DocumentImageBitmap = new Bitmap(stream);
                OnPropertyChanged(nameof(IsViewingDocument));
                DocumentMessage = $"{document.Name} chargé ({content.Content.Length:N0} octets).";
            }
            else if (IsDocumentPdf)
            {
                var tempPath = CreateTempDocumentPath(displayName);
                await File.WriteAllBytesAsync(tempPath, content.Content, ct);
                CleanupTempPdfFile();
                _lastPdfTempPath = tempPath;
                DocumentFilePath = tempPath;
                OnPropertyChanged(nameof(IsDocumentFile));
                OnPropertyChanged(nameof(IsViewingDocument));
                DocumentMessage = $"PDF prêt à être consulté.";
            }
            else
            {
                var tempPath = CreateTempDocumentPath(displayName);
                await File.WriteAllBytesAsync(tempPath, content.Content, ct);
                CleanupTempPdfFile();
                _lastPdfTempPath = tempPath;
                DocumentFilePath = tempPath;
                OnPropertyChanged(nameof(IsDocumentFile));
                OnPropertyChanged(nameof(IsViewingDocument));
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

    [RelayCommand]
    private void OpenSelectedDocumentExternally()
    {
        if (string.IsNullOrWhiteSpace(DocumentFilePath))
        {
            return;
        }

        try
        {
            var psi = new ProcessStartInfo(DocumentFilePath) { UseShellExecute = true };
            Process.Start(psi);
            DocumentMessage = "Document ouvert dans le visualiseur système.";
        }
        catch
        {
            DocumentMessage = $"Document disponible dans : {DocumentFilePath}";
        }
    }
    
    private void ClearDocumentPreview()
    {
        var previousBitmap = DocumentImageBitmap;
        DocumentImageBitmap = null;
        DocumentMimeType = null;
        ViewingDocumentName = null;
        DocumentFilePath = null;
        previousBitmap?.Dispose();
        OnPropertyChanged(nameof(IsViewingDocument));
        OnPropertyChanged(nameof(IsDocumentImage));
        OnPropertyChanged(nameof(IsDocumentPdf));
        OnPropertyChanged(nameof(IsDocumentFile));
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
        catch (Exception ex)
        {
            Trace.TraceWarning("Failed to clean temporary PDF file {0}: {1}", _lastPdfTempPath, ex.Message);
        }
        finally
        {
            _lastPdfTempPath = null;
        }
    }

    private static string CreateTempDocumentPath(string documentName)
    {
        var safeName = string.Concat(
            Path.GetFileNameWithoutExtension(documentName)
                .Select(c => Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c));
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "document";
        }

        var extension = Path.GetExtension(documentName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        return Path.Combine(Path.GetTempPath(), $"{TempDocumentPrefix}{safeName}_{Guid.NewGuid():N}{extension}");
    }

    private static string ResolveDisplayDocumentName(string contentName, string documentName)
    {
        if (!string.IsNullOrWhiteSpace(contentName) && !string.IsNullOrWhiteSpace(Path.GetExtension(contentName)))
        {
            return contentName;
        }

        return string.IsNullOrWhiteSpace(documentName) ? contentName : documentName;
    }

    private static string? ResolveDocumentMimeType(string? contentType, string documentName)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType;
        }

        return Path.GetExtension(documentName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => contentType
        };
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
