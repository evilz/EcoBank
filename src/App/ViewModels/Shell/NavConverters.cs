using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EcoBank.App.ViewModels.Shell;

/// <summary>
/// Convertit la clé du tab courant en bool IsVisible pour chaque icône.
/// </summary>
public sealed class NavKeyToBoolConverter : IValueConverter
{
    public static readonly NavKeyToBoolConverter IsHome     = new("home");
    public static readonly NavKeyToBoolConverter IsCards    = new("cards");
    public static readonly NavKeyToBoolConverter IsContact  = new("contact");
    public static readonly NavKeyToBoolConverter IsTransfer = new("transfer");
    public static readonly NavKeyToBoolConverter IsMenu     = new("menu");

    private readonly string _key;
    private NavKeyToBoolConverter(string key) => _key = key;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == _key;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit la clé du tab sélectionné (SelectedTab.Key) en couleur active/inactive
/// pour chaque élément de navigation.
/// </summary>
public sealed class NavActiveColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Active   = new(Color.Parse("#1A7F4F"));
    private static readonly SolidColorBrush Inactive = new(Color.Parse("#7A7F85"));

    public static readonly NavActiveColorConverter ForHome     = new("home");
    public static readonly NavActiveColorConverter ForCards    = new("cards");
    public static readonly NavActiveColorConverter ForContact  = new("contact");
    public static readonly NavActiveColorConverter ForTransfer = new("transfer");
    public static readonly NavActiveColorConverter ForMenu     = new("menu");

    private readonly string _myKey;
    private NavActiveColorConverter(string key) => _myKey = key;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string selectedKey && selectedKey == _myKey ? Active : Inactive;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

