using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using AvaloniaApp = EcoBank.App.App;

namespace EcoBank.Android;

[Application]
public class MainApplication : AvaloniaAndroidApplication<AvaloniaApp>
{
    public MainApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .LogToTrace();
}
