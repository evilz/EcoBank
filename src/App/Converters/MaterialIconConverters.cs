using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IconPacks.Avalonia.Material.Converter;

namespace EcoBank.App.Converters;

/// <summary>
/// Convertit PackIconMaterialKind vers une image via le converter du package.
/// </summary>
public sealed class MaterialKindToImageConverter : IValueConverter
{
    public static readonly MaterialKindToImageConverter Instance = new();

    private static readonly IValueConverter Inner = new PackIconMaterialKindToImageConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Inner.Convert(value, targetType, parameter, culture);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class MaterialKindToGeometryConverter : IValueConverter
{
    public static readonly MaterialKindToGeometryConverter Instance = new();

    private static readonly PackIconMaterialKindToImageConverter Inner = new();
    private static readonly MethodInfo? GetPathDataMethod =
        typeof(PackIconMaterialKindToImageConverter).GetMethod(
            "GetPathData",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pathData = GetPathDataMethod?.Invoke(Inner, [value]);
        return pathData is null ? null : StreamGeometry.Parse(pathData.ToString() ?? "");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
