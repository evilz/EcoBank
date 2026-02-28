using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(EcoBank.App.Tests.Infrastructure.TestAppBuilder))]

namespace EcoBank.App.Tests.Infrastructure;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            });
}

public sealed class TestApplication : Application;
