# Avalonia Platform Guide

## Official Docs To Check

Avalonia documents support for Windows, macOS, Linux, iOS, Android, and WebAssembly:

- Supported platforms: https://docs.avaloniaui.net/docs/overview/supported-platforms
- Cross-platform solution setup: https://docs.avaloniaui.net/docs/app-development/cross-platform-solution-setup
- Cross-platform architecture: https://docs.avaloniaui.net/docs/guides/building-cross-platform-applications/

Use these docs when exact platform tiers, CLI templates, target frameworks, or packaging steps matter. Treat installed templates and SDK workloads as the source of truth for the local machine.

## Solution Shape

Prefer this structure for new apps:

```text
src/
  App/                 shared Avalonia UI, styles, resources, view models
  Core/                domain models, use cases, app abstractions
  Infrastructure/      shared infrastructure that is not platform-native
  Desktop/             Windows/macOS/Linux executable head
  Android/             Android executable head
  iOS/                 iOS executable head
  Browser/             WebAssembly/browser head
tests/
  App.Tests/
  Core.Tests/
```

For small apps, keep fewer projects. For production apps, separate domain logic from UI so platform heads stay thin.

## Targeting Guidelines

- Desktop: validate native window behavior, icons, menus, resize constraints, drag/drop, file dialogs, and packaging metadata.
- Android: validate permissions, lifecycle, back button, virtual keyboard, safe areas, density, status/navigation bars, and emulator/device screenshots.
- iOS: validate safe areas, keyboard overlap, entitlements, app icons, provisioning, and device-specific layouts.
- WebAssembly: validate browser asset loading, startup size, async work, unsupported APIs, URL routing, and local storage/network constraints.

## MVVM And DI

- Keep view models platform-neutral.
- Inject platform services through interfaces such as `IFilePicker`, `ISecureStorage`, `INotificationService`, `IExternalBrowser`, or `IDeviceInfo`.
- Register default/shared services in the shared app project and override native implementations in platform heads.
- Keep navigation state in view models or navigation services, not in code-behind.

## XAML Patterns

- Use `x:DataType` and compiled bindings when the repo uses them.
- Use `DataTemplate` and view locator patterns for view-model-first navigation.
- Use `DynamicResource` for theme-aware values and `StaticResource` for fixed tokens.
- Use `ScrollViewer` around mobile-sized or vertically dense flows.
- Constrain desktop mockups with `MaxWidth` while allowing window resizing.
- Reset scroll/focus when switching auth or wizard states if the same scroll container is reused.

## Assets And Icons

- Put shared images, fonts, and icons under the shared app assets folder with `AvaloniaResource`.
- Set the desktop window icon in XAML/code and set the executable `ApplicationIcon` in the desktop project file.
- Use platform-specific icon pipelines for Android/iOS store assets and launchers.
- Prefer vector icons through an established icon package only if the app already depends on one.

## Testing Checklist

Run only the checks that match the risk:

```powershell
dotnet build src/App/App.csproj -c Debug
dotnet build src/Desktop/Desktop.csproj -c Debug
dotnet build src/Android/Android.csproj -c Debug
dotnet build src/Browser/Browser.csproj -c Debug
dotnet test tests/App.Tests/App.Tests.csproj -c Debug --no-restore
```

When a build fails due to environment setup, report the exact blocker: missing workload, unsupported JDK, missing Android SDK, missing Xcode, browser tooling, or locked running process.

## Common Failure Modes

- XAML property exists in WPF but not Avalonia.
- A resource is not included as `AvaloniaResource`.
- A desktop-only file/process API is called from mobile or WebAssembly.
- Fixed mobile dimensions make desktop layouts look narrow or clipped.
- A shared scroll container preserves offset across wizard/auth states.
- Platform heads reference different package versions.
- Android build uses an unsupported JDK version.
- Desktop app launches without native title bar because custom chrome was left enabled.
