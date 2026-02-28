using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EcoBank.App.Views.Auth;
using Xunit;

namespace EcoBank.App.Tests.Views;

public class LoginViewTests
{
    [AvaloniaFact]
    public void LoginView_RendersExpectedInputFields()
    {
        var view = new LoginView();

        var textBoxes = view.GetVisualDescendants().OfType<TextBox>().ToList();

        Assert.True(textBoxes.Count >= 3);
        Assert.Contains(textBoxes, box => AutomationProperties.GetName(box) == "Client ID");
        Assert.Contains(textBoxes, box => AutomationProperties.GetName(box) == "Client Secret");
        Assert.Contains(textBoxes, box => AutomationProperties.GetName(box) == "App User ID");
    }
}
