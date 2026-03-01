using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EcoBank.App.Views.Auth;
using Xunit;

namespace EcoBank.App.Tests.Views;

public class LoginViewTests
{
    private static Window CreateWindowWithView()
    {
        var window = new Window { Width = 800, Height = 600 };
        window.Content = new LoginView();
        window.Show();
        return window;
    }

    [AvaloniaFact]
    public void LoginView_RendersWithoutCrashing()
    {
        var window = CreateWindowWithView();

        var descendants = window.GetVisualDescendants().ToList();
        Assert.NotEmpty(descendants);
    }

    [AvaloniaFact]
    public void LoginView_ContainsOtpPinInput()
    {
        var window = CreateWindowWithView();

        var descendants = window.GetVisualDescendants().ToList();
        var otpInput = descendants.OfType<OtpPinInput>().FirstOrDefault();
        Assert.NotNull(otpInput);
    }

    [AvaloniaFact]
    public void LoginView_AddProfileState_ContainsAccessibleTextBoxes()
    {
        var window = CreateWindowWithView();

        var descendants = window.GetVisualDescendants().ToList();
        var textBoxes = descendants.OfType<TextBox>().ToList();

        var automationNames = textBoxes
            .Select(tb => Avalonia.Automation.AutomationProperties.GetName(tb))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        Assert.Contains("Client ID", automationNames);
        Assert.Contains("Client Secret", automationNames);
        Assert.Contains("App User ID", automationNames);
        Assert.Contains("Code PIN (Nouveau)", automationNames);
    }
}
