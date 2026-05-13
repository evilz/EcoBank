using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace EcoBank.App.ViewModels.Profile;

/// <summary>
/// Converts a byte[] to an Avalonia Bitmap for displaying document images.
/// </summary>
public sealed class BytesToBitmapConverter : IValueConverter
{
    public static readonly BytesToBitmapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] { Length: > 0 } bytes)
        {
            try
            {
                using var stream = new MemoryStream(bytes);
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
