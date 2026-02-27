using Avalonia;
using Avalonia.iOS;
using Foundation;
using AvaloniaApp = EcoBank.App.App;

namespace EcoBank.iOS;

[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<AvaloniaApp>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .LogToTrace();
}
