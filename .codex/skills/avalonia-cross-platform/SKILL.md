---
name: avalonia-cross-platform
description: Build, debug, modernize, or review Avalonia applications targeting Windows, macOS, Linux, iOS, Android, and WebAssembly. Use when Codex works on Avalonia XAML/C# UI, MVVM structure, cross-platform project setup, platform-specific heads, assets, native integration, responsive layouts, desktop/mobile/browser testing, packaging, or migration from WPF/WinUI/MAUI-style patterns to Avalonia.
---

# Avalonia Cross-Platform

## Workflow

1. Inspect the solution shape before editing.
   - Identify shared UI projects, platform heads, target frameworks, package versions, and existing MVVM/navigation patterns.
   - Prefer existing app architecture over introducing new frameworks.

2. Keep shared code shared.
   - Put views, view models, styles, converters, services, and domain logic in the shared Avalonia project when possible.
   - Keep platform heads thin: app bootstrap, permissions, manifests, native services, icons, packaging, and platform-specific entry points.
   - Use interfaces for native capabilities; bind implementations in each platform head.

3. Design XAML for every target.
   - Avoid desktop-only assumptions unless the app explicitly excludes mobile/browser.
   - Use responsive layout primitives (`Grid`, `DockPanel`, `ScrollViewer`, adaptive widths, min/max constraints).
   - Verify pointer, touch, keyboard, focus, and screen size behavior.
   - Keep fixed dimensions only for controls with truly fixed formats.

4. Handle platform differences deliberately.
   - Use conditional target frameworks, partial classes, DI registrations, or platform service implementations.
   - Do not scatter platform checks through views unless the UI itself must change.
   - For WebAssembly, avoid direct filesystem/process APIs and long blocking work on the UI thread.
   - For iOS/Android, account for safe areas, permissions, virtual keyboards, lifecycle, and packaging assets.

5. Validate with the cheapest reliable loop first.
   - Build the shared project to catch XAML and compiled binding errors.
   - Build desktop for fast runtime checks.
   - Build platform heads touched by the change.
   - Run Android/iOS/browser emulators only when the touched behavior depends on that platform.

## Commands

Prefer repo-local commands and installed SDKs. Common checks:

```powershell
dotnet build <shared-app-project> -c Debug
dotnet build <desktop-project> -c Debug
dotnet test <test-project> -c Debug --no-restore
dotnet workload list
dotnet new list avalonia
```

For Android emulator QA, use the Android testing skill when available. For browser/WebAssembly UI checks, use browser automation when a local dev server or static output is available.

## Editing Rules

- Use compiled binding errors as design feedback; fix bindings instead of disabling typed bindings.
- Do not use code-behind for business logic. Use it only for view lifecycle, focus, scroll reset, native interop, or visual behavior that is hard to express declaratively.
- Keep styles and design tokens centralized when the app already has a style system.
- Prefer `AvaloniaResource` assets for shared images/icons; use platform packaging metadata for app icons and store assets.
- Avoid assuming WPF APIs exist in Avalonia. Check Avalonia equivalents for templates, resources, routed events, data templates, and platform services.

## Reference

Read `references/platform-guide.md` when the task involves creating a new Avalonia solution, changing target frameworks, adding platform heads, touching native assets, or debugging platform-specific behavior.
