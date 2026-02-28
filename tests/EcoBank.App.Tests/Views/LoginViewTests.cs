using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EcoBank.App.Views.Auth;

namespace EcoBank.App.Tests.Views;

public class LoginViewTests
{
    [AvaloniaFact]
    public void LoginView_RendersExpectedInputFields()
    {
        var view = new LoginView();

        var textBoxes = view.GetVisualDescendants().OfType<TextBox>().ToList();

        Assert.True(textBoxes.Count >= 3);
        Assert.Contains(textBoxes, box => box.Watermark?.ToString() == "Votre Client ID Xpollens");
        Assert.Contains(textBoxes, box => box.Watermark?.ToString() == "Votre Client Secret");
        Assert.Contains(textBoxes, box => box.Watermark?.ToString() == "Votre App User ID");
    }
}
