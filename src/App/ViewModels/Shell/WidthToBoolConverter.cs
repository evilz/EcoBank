using System.Globalization;
using Avalonia.Data.Converters;

namespace EcoBank.App.ViewModels.Shell;

public sealed class WidthToBoolConverter : IValueConverter
{
    public static readonly WidthToBoolConverter IsWide = new() { Threshold = 600, WideReturnsTrue = true };
    public static readonly WidthToBoolConverter IsNarrow = new() { Threshold = 600, WideReturnsTrue = false };

    public double Threshold { get; init; } = 600;
    public bool WideReturnsTrue { get; init; } = true;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width)
            return WideReturnsTrue ? width >= Threshold : width < Threshold;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
