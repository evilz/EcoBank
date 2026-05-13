using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IconPacks.Avalonia.Material;

namespace EcoBank.App.ViewModels.Shell;

/// <summary>
/// Convertit la clé du tab courant en bool IsVisible pour chaque icône.
/// </summary>
public sealed class NavKeyToBoolConverter : IValueConverter
{
    public static readonly NavKeyToBoolConverter IsHome       = new("home");
    public static readonly NavKeyToBoolConverter IsCards      = new("cards");
    public static readonly NavKeyToBoolConverter IsContact    = new("contact");
    public static readonly NavKeyToBoolConverter IsOperations = new("operations");
    public static readonly NavKeyToBoolConverter IsMenu       = new("menu");

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
    private static readonly SolidColorBrush Active   = new(Color.Parse("#168246"));
    private static readonly SolidColorBrush Inactive = new(Color.Parse("#8A9AA5"));

    public static readonly NavActiveColorConverter ForHome       = new("home");
    public static readonly NavActiveColorConverter ForCards      = new("cards");
    public static readonly NavActiveColorConverter ForContact    = new("contact");
    public static readonly NavActiveColorConverter ForOperations = new("operations");
    public static readonly NavActiveColorConverter ForMenu       = new("menu");

    private readonly string _myKey;
    private NavActiveColorConverter(string key) => _myKey = key;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string selectedKey && selectedKey == _myKey ? Active : Inactive;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit (tabKey, selectedKey) → couleur active/inactive.
/// Utilisé dans les ItemsControl data-driven pour la bottom bar.
/// </summary>
public sealed class NavTabActiveColorConverter : IMultiValueConverter
{
    public static readonly NavTabActiveColorConverter Instance = new();

    private static readonly SolidColorBrush Active   = new(Color.Parse("#168246"));
    private static readonly SolidColorBrush Inactive = new(Color.Parse("#8A9AA5"));

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2
            && values[0] is string tabKey
            && values[1] is string selectedKey)
            return tabKey == selectedKey ? Active : Inactive;
        return Inactive;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit la clé de navigation en icône Material pour éviter les glyphes emoji.
/// </summary>
public sealed class NavTabIconConverter : IValueConverter
{
    public static readonly NavTabIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "home" => PackIconMaterialKind.Home,
            "cards" => PackIconMaterialKind.CreditCardOutline,
            "contact" => PackIconMaterialKind.BankTransfer,
            "operations" => PackIconMaterialKind.ChartLine,
            "menu" => PackIconMaterialKind.AccountOutline,
            _ => PackIconMaterialKind.CircleOutline
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
