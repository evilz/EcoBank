using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace EcoBank.App.Views.Auth;

public partial class OtpPinInput : UserControl
{
    private readonly TextBox[] _inputs;
    private bool _isUpdatingProgrammatically;

    public static readonly StyledProperty<string> OtpValueProperty =
        AvaloniaProperty.Register<OtpPinInput, string>(
            nameof(OtpValue),
            defaultValue: string.Empty);

    public string OtpValue
    {
        get => GetValue(OtpValueProperty);
        set => SetValue(OtpValueProperty, value);
    }

    public event EventHandler? OtpChanged;

    public OtpPinInput()
    {
        InitializeComponent();
        
        _inputs = new[] 
        { 
            OtpInput1, 
            OtpInput2, 
            OtpInput3, 
            OtpInput4 
        };

        // Set up input handlers for each textbox
        foreach (var input in _inputs)
        {
            input.TextInput += OnOtpTextInput;
            input.KeyDown += OnOtpKeyDown;
            input.TextChanged += OnOtpTextChanged;
        }

        // Listen for property changes
        OtpValueProperty.Changed.AddClassHandler<OtpPinInput>((control, args) =>
        {
            if (args.NewValue is string newValue && !control._isUpdatingProgrammatically)
            {
                control.SetOtpValue(newValue);
            }
        });
    }

    private void OnOtpTextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        var currentIndex = Array.IndexOf(_inputs, textBox);
        if (currentIndex < 0) return;

        var incoming = e.Text ?? string.Empty;
        var digits = incoming.Where(char.IsDigit).ToArray();
        if (digits.Length == 0)
        {
            textBox.Clear();
            e.Handled = true;
            RaiseOtpChanged();
            return;
        }

        _isUpdatingProgrammatically = true;
        for (var i = 0; i < digits.Length && currentIndex + i < _inputs.Length; i++)
        {
            _inputs[currentIndex + i].Text = digits[i].ToString();
        }
        _isUpdatingProgrammatically = false;

        var nextIndex = Math.Min(currentIndex + digits.Length, _inputs.Length - 1);
        _inputs[nextIndex].Focus();

        RaiseOtpChanged();
        e.Handled = true;
    }

    private void OnOtpTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (_isUpdatingProgrammatically) return;

        var currentIndex = Array.IndexOf(_inputs, textBox);
        if (currentIndex < 0) return;

        var text = textBox.Text ?? string.Empty;
        var digits = text.Where(char.IsDigit).ToArray();

        if (digits.Length == 0)
        {
            textBox.Text = string.Empty;
            RaiseOtpChanged();
            return;
        }

        if (digits.Length > 1)
        {
            _isUpdatingProgrammatically = true;
            for (var i = 0; i < digits.Length && currentIndex + i < _inputs.Length; i++)
            {
                _inputs[currentIndex + i].Text = digits[i].ToString();
            }
            _isUpdatingProgrammatically = false;

            var nextIndex = Math.Min(currentIndex + digits.Length, _inputs.Length - 1);
            _inputs[nextIndex].Focus();
        }
        else
        {
            textBox.Text = digits[0].ToString();
            if (currentIndex < _inputs.Length - 1)
            {
                _inputs[currentIndex + 1].Focus();
            }
        }

        RaiseOtpChanged();
    }

    private void OnOtpKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        var currentIndex = System.Array.IndexOf(_inputs, textBox);

        if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            if (string.IsNullOrEmpty(textBox.Text) && currentIndex > 0)
            {
                // Move to previous field when deleting empty field
                _inputs[currentIndex - 1].Focus();
                _inputs[currentIndex - 1].Clear();
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                // Clear current field
                textBox.Clear();
            }

            RaiseOtpChanged();
            e.Handled = true;
        }
        else if (e.Key == Key.Left && currentIndex > 0)
        {
            _inputs[currentIndex - 1].Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Right && currentIndex < _inputs.Length - 1)
        {
            _inputs[currentIndex + 1].Focus();
            e.Handled = true;
        }
    }

    private void RaiseOtpChanged()
    {
        _isUpdatingProgrammatically = true;
        var newValue = GetOtpValue();
        OtpValue = newValue;
        _isUpdatingProgrammatically = false;
        OtpChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Get the complete OTP value from all four inputs.
    /// </summary>
    public string GetOtpValue()
    {
        return string.Concat(_inputs.Select(i => i.Text ?? ""));
    }

    /// <summary>
    /// Clear all OTP input fields.
    /// </summary>
    public void Clear()
    {
        _isUpdatingProgrammatically = true;
        foreach (var input in _inputs)
        {
            input.Clear();
        }
        OtpValue = string.Empty;
        _isUpdatingProgrammatically = false;
        _inputs[0].Focus();
    }

    /// <summary>
    /// Set OTP value programmatically.
    /// </summary>
    public void SetOtpValue(string value)
    {
        _isUpdatingProgrammatically = true;
        value = value ?? string.Empty;
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i].Text = i < value.Length ? value[i].ToString() : string.Empty;
        }
        _isUpdatingProgrammatically = false;
    }
}
