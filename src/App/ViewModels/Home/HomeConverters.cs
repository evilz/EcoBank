using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IconPacks.Avalonia.Material;

namespace EcoBank.App.ViewModels.Home;

/// <summary>
/// Convertit le montant (positif/négatif) en couleur crédit/débit.
/// </summary>
public sealed class AmountToColorConverter : IValueConverter
{
    public static readonly AmountToColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
            return amount >= 0
                ? new SolidColorBrush(Color.Parse("#1E7F4F"))
                : new SolidColorBrush(Color.Parse("#1B1D1F"));
        return new SolidColorBrush(Color.Parse("#1B1D1F"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit la catégorie d'une opération en couleur d'avatar.
/// </summary>
public sealed class CategoryToColorConverter : IValueConverter
{
    public static readonly CategoryToColorConverter Instance = new();

    private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "virement",     "#2E8B6E" },   // vert foncé  (virements entrants)
        { "salaire",      "#2E8B6E" },
        { "courses",      "#E07020" },   // orange      (courses/alimentation)
        { "alimentation", "#E07020" },
        { "transport",    "#5C6B7A" },   // gris-bleu   (transport/auto)
        { "auto",         "#5C6B7A" },
        { "loisirs",      "#8B46A0" },   // violet      (loisirs)
        { "abonnement",   "#1565C0" },   // bleu        (abonnements)
        { "sante",        "#C62828" },   // rouge       (santé)
        { "banque",       "#37474F" },   // gris foncé  (opérations bancaires)
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value as string ?? "";
        if (_map.TryGetValue(category, out var hex))
            return new SolidColorBrush(Color.Parse(hex));
        return new SolidColorBrush(Color.Parse("#7A8A8A")); // gris par défaut
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit la catégorie d'une opération en icône Material.
/// </summary>
public sealed class CategoryToIconConverter : IValueConverter
{
    public static readonly CategoryToIconConverter Instance = new();

    private static readonly Dictionary<string, PackIconMaterialKind> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "virement",     PackIconMaterialKind.BankTransfer },
        { "salaire",      PackIconMaterialKind.BriefcaseOutline },
        { "courses",      PackIconMaterialKind.CartOutline },
        { "alimentation", PackIconMaterialKind.FoodOutline },
        { "transport",    PackIconMaterialKind.CarOutline },
        { "auto",         PackIconMaterialKind.CarOutline },
        { "loisirs",      PackIconMaterialKind.GamepadVariantOutline },
        { "abonnement",   PackIconMaterialKind.RepeatVariant },
        { "sante",        PackIconMaterialKind.MedicalBag },
        { "banque",       PackIconMaterialKind.BankOutline },
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value as string ?? "";
        return _map.TryGetValue(category, out var kind)
            ? kind
            : PackIconMaterialKind.CreditCardOutline;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convertit IsBalanceVisible (bool) en icône Eye ou EyeOff.
/// </summary>
public sealed class BoolToEyeIconConverter : IValueConverter
{
    public static readonly BoolToEyeIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? PackIconMaterialKind.Eye : PackIconMaterialKind.EyeOff;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

