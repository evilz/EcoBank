using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace EcoBank.App.ViewModels.Profile;

/// <summary>
/// Converts a boolean toggle state to a background color (green = on, grey = off).
/// </summary>
public sealed class BoolToToggleColorConverter : IValueConverter
{
    public static readonly BoolToToggleColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.Parse("#168246"))
            : new SolidColorBrush(Color.Parse("#EEF2F4"));

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a boolean toggle state to HorizontalAlignment for the toggle thumb.
/// true = Right (active), false = Left (inactive).
/// </summary>
public sealed class BoolToToggleAlignConverter : IValueConverter
{
    public static readonly BoolToToggleAlignConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
