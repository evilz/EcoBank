using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using AvaloniaApp = EcoBank.App.App;

[assembly: SupportedOSPlatform("browser")]

namespace EcoBank.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args) =>
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AvaloniaApp>()
            .LogToTrace();
}
