using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using AvaloniaApp = EcoBank.App.App;

namespace EcoBank.Android;

[Activity(
    Label = "EcoBank",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<AvaloniaApp>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .LogToTrace();
}
