namespace EcoBank.App.Services;

/// <summary>
/// Allows child ViewModels to trigger shell-level tab navigation.
/// MainShellViewModel registers its SelectTabByKey callback on startup.
/// </summary>
public sealed class ShellNavigationContext
{
    public Action<string>? SelectTabByKey { get; set; }

    public void GoToHome() => SelectTabByKey?.Invoke("home");
    public void GoToAccounts() => SelectTabByKey?.Invoke("accounts");
    public void GoToPayments() => SelectTabByKey?.Invoke("payments");
    public void GoToCards() => SelectTabByKey?.Invoke("cards");
    public void GoToProfile() => SelectTabByKey?.Invoke("profile");
}
