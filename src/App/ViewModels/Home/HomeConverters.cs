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

    private static readonly SolidColorBrush CreditBrush  = new(Color.Parse("#1E7F4F"));
    private static readonly SolidColorBrush DebitBrush   = new(Color.Parse("#1B1D1F"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
            return amount >= 0 ? CreditBrush : DebitBrush;
        return DebitBrush;
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

    private static readonly Dictionary<string, SolidColorBrush> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "virement",     new SolidColorBrush(Color.Parse("#2E8B6E")) },
        { "salaire",      new SolidColorBrush(Color.Parse("#2E8B6E")) },
        { "courses",      new SolidColorBrush(Color.Parse("#E07020")) },
        { "alimentation", new SolidColorBrush(Color.Parse("#E07020")) },
        { "transport",    new SolidColorBrush(Color.Parse("#5C6B7A")) },
        { "auto",         new SolidColorBrush(Color.Parse("#5C6B7A")) },
        { "loisirs",      new SolidColorBrush(Color.Parse("#8B46A0")) },
        { "abonnement",   new SolidColorBrush(Color.Parse("#1565C0")) },
        { "sante",        new SolidColorBrush(Color.Parse("#C62828")) },
        { "banque",       new SolidColorBrush(Color.Parse("#37474F")) },
    };
    private static readonly SolidColorBrush DefaultBrush = new(Color.Parse("#7A8A8A"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value as string ?? "";
        return _map.TryGetValue(category, out var brush) ? brush : DefaultBrush;
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

