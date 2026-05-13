using CommunityToolkit.Mvvm.ComponentModel;
using EcoBank.Core.Application;

namespace EcoBank.App.ViewModels.Contact;

/// <summary>
/// Placeholder pour la section Contact - sera implemente dans une future version.
/// </summary>
public partial class ContactViewModel : ViewModelBase
{
    private readonly UserContext _userContext;

    [ObservableProperty]
    private string _message = "La section Contact sera disponible prochainement.";

    public string UserDisplayName =>
        _userContext.SelectedUser is { } u
            ? $"{u.FirstName} {u.LastName}".Trim().NullIfEmpty() ?? u.AppUserId
            : "Utilisateur";

    public ContactViewModel(UserContext userContext)
    {
        _userContext = userContext;
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
