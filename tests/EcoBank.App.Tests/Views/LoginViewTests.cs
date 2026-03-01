using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EcoBank.App.Views.Auth;
using Xunit;

namespace EcoBank.App.Tests.Views;

public class LoginViewTests
{
    [AvaloniaFact]
    public void LoginView_RendersWithoutCrashing()
    {
        var view = new LoginView();

        // The view should instantiate and render without throwing
        var descendants = view.GetVisualDescendants().ToList();
        Assert.NotNull(descendants);
    }
}
