using Avalonia;
using Avalonia.Controls;
using EcoBank.App.Views.Auth;

namespace EcoBank.App.Behaviors;

/// <summary>
/// Attached behavior for OTP PIN input to bind value to a ViewModel property.
/// </summary>
public static class OtpPinInputBehavior
{
    public static readonly AttachedProperty<string> OtpValueProperty =
        AvaloniaProperty.RegisterAttached<Control, string>(
            "OtpValue",
            typeof(OtpPinInputBehavior),
            defaultValue: string.Empty,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    static OtpPinInputBehavior()
    {
        OtpValueProperty.Changed.AddClassHandler<OtpPinInput>((control, args) =>
        {
            if (args.NewValue is string newValue && control.GetOtpValue() != newValue)
            {
                control.SetOtpValue(newValue);
            }
        });
    }

    public static string GetOtpValue(OtpPinInput control)
    {
        return control.GetValue(OtpValueProperty);
    }

    public static void SetOtpValue(OtpPinInput control, string value)
    {
        control.SetValue(OtpValueProperty, value);
    }
}

