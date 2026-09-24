using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// La pastilla de estado de las tablas. Se le pasa el booleano y las dos palabras:
// asi la misma pieza sirve para Activo/Inactivo, Aplica/No aplica o lo que venga, y el
// color siempre sale de la paleta.
public partial class SharedStatePill : UserControl
{
    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
        nameof(IsOn),
        typeof(bool),
        typeof(SharedStatePill),
        new PropertyMetadata(false, OnStateChanged));

    public static readonly DependencyProperty OnTextProperty = DependencyProperty.Register(
        nameof(OnText),
        typeof(string),
        typeof(SharedStatePill),
        new PropertyMetadata("Activo", OnStateChanged));

    public static readonly DependencyProperty OffTextProperty = DependencyProperty.Register(
        nameof(OffText),
        typeof(string),
        typeof(SharedStatePill),
        new PropertyMetadata("Inactivo", OnStateChanged));

    public SharedStatePill()
    {
        InitializeComponent();
        Paint();
    }

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public string OnText
    {
        get => (string)GetValue(OnTextProperty);
        set => SetValue(OnTextProperty, value);
    }

    public string OffText
    {
        get => (string)GetValue(OffTextProperty);
        set => SetValue(OffTextProperty, value);
    }

    private static void OnStateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((SharedStatePill)dependencyObject).Paint();
    }

    // Los colores se piden con DynamicResource para que sigan al tema
    private void Paint()
    {
        var sufijo = IsOn ? "On" : "Off";

        PillBorder.SetResourceReference(BackgroundProperty, $"BrushIndexPill{sufijo}Back");
        PillBorder.SetResourceReference(BorderBrushProperty, $"BrushIndexPill{sufijo}Border");
        PillDot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, $"BrushIndexPill{sufijo}Text");
        PillText.SetResourceReference(ForegroundProperty, $"BrushIndexPill{sufijo}Text");
        PillText.Text = IsOn ? OnText : OffText;
    }
}
