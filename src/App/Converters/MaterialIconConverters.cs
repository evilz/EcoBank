using System.Globalization;
using Avalonia.Data.Converters;
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
