using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Material.Icons;

namespace EcoBank.App.ViewModels.Home;

/// <summary>
/// Convertit le montant (positif/négatif) en couleur crédit/débit.
/// </summary>
public sealed class AmountToColorConverter : IValueConverter
{
    public static readonly AmountToColorConverter Instance = new();

    private static readonly SolidColorBrush CreditBrush  = new(Color.Parse("#168246"));
    private static readonly SolidColorBrush DebitBrush   = new(Color.Parse("#101820"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
            return amount >= 0 ? CreditBrush : DebitBrush;
        return DebitBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class MoneyTextConverter : IMultiValueConverter
{
    public static readonly MoneyTextConverter Instance = new();

    private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0 || values[0] is null || ReferenceEquals(values[0], AvaloniaProperty.UnsetValue))
            return "";

        var amount = values[0] switch
        {
            decimal d => d,
            double d => (decimal)d,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => 0m
        };

        var currency = values.Count > 1 ? values[1]?.ToString() : "EUR";
        var symbol = string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase) ? "€" : currency ?? "";
        return $"{amount.ToString("N2", FrenchCulture)} {symbol}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class MaskedMoneyTextConverter : IValueConverter
{
    public static readonly MaskedMoneyTextConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var currency = value?.ToString();
        var symbol = string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase) ? "€" : currency ?? "";
        return string.IsNullOrWhiteSpace(symbol) ? "••••••" : $"•••••• {symbol}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SignedMoneyTextConverter : IMultiValueConverter
{
    public static readonly SignedMoneyTextConverter Instance = new();

    private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0 || values[0] is null || ReferenceEquals(values[0], AvaloniaProperty.UnsetValue))
            return "";

        var amount = values[0] switch
        {
            decimal d => d,
            double d => (decimal)d,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => 0m
        };

        var currency = values.Count > 1 ? values[1]?.ToString() : "EUR";
        var symbol = string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase) ? "€" : currency ?? "";
        var sign = amount > 0 ? "+ " : amount < 0 ? "- " : "";
        return $"{sign}{Math.Abs(amount).ToString("N2", FrenchCulture)} {symbol}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
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

    private static readonly Dictionary<string, MaterialIconKind> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "virement",     MaterialIconKind.BankTransfer },
        { "salaire",      MaterialIconKind.BriefcaseOutline },
        { "courses",      MaterialIconKind.CartOutline },
        { "alimentation", MaterialIconKind.FoodOutline },
        { "transport",    MaterialIconKind.CarOutline },
        { "auto",         MaterialIconKind.CarOutline },
        { "loisirs",      MaterialIconKind.GamepadVariantOutline },
        { "abonnement",   MaterialIconKind.RepeatVariant },
        { "sante",        MaterialIconKind.MedicalBag },
        { "banque",       MaterialIconKind.BankOutline },
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value as string ?? "";
        return _map.TryGetValue(category, out var kind)
            ? kind
            : MaterialIconKind.CreditCardOutline;
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
        => value is true ? MaterialIconKind.Eye : MaterialIconKind.EyeOff;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

