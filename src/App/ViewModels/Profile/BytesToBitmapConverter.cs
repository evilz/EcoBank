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
    private readonly object _sync = new();
    private byte[]? _cachedBytes;
    private Bitmap? _cachedBitmap;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] { Length: > 0 } bytes)
        {
            lock (_sync)
            {
                try
                {
                    if (_cachedBytes is not null && bytes.AsSpan().SequenceEqual(_cachedBytes))
                    {
                        return _cachedBitmap;
                    }

                    using var stream = new MemoryStream(bytes);
                    var bitmap = new Bitmap(stream);
                    var previous = _cachedBitmap;
                    _cachedBytes = bytes.ToArray();
                    _cachedBitmap = bitmap;
                    previous?.Dispose();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
