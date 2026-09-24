using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedCatalog;

// La pastilla de estado del catalogo. Se le pasa el booleano y los dos textos.
public partial class SharedCatalogPill : UserControl
{
    public static readonly DependencyProperty IsOnProperty =
        DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(SharedCatalogPill),
            new PropertyMetadata(false, EstadoCambio));

    public static readonly DependencyProperty OnTextProperty =
        DependencyProperty.Register(nameof(OnText), typeof(string), typeof(SharedCatalogPill),
            new PropertyMetadata("Activo", EstadoCambio));

    public static readonly DependencyProperty OffTextProperty =
        DependencyProperty.Register(nameof(OffText), typeof(string), typeof(SharedCatalogPill),
            new PropertyMetadata("Inactivo", EstadoCambio));

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

    public SharedCatalogPill()
    {
        InitializeComponent();
        Pintar();
    }

    private static void EstadoCambio(DependencyObject elemento, DependencyPropertyChangedEventArgs e)
    {
        ((SharedCatalogPill)elemento).Pintar();
    }

    // Se pide el pincel por nombre para que siga el tema si se cambia en caliente
    private void Pintar()
    {
        PillBorder.SetResourceReference(
            System.Windows.Controls.Border.BackgroundProperty,
            IsOn ? "BrushCatalogPillOnBack" : "BrushCatalogPillOffBack");

        PillText.SetResourceReference(
            TextBlock.ForegroundProperty,
            IsOn ? "BrushCatalogPillOnText" : "BrushCatalogPillOffText");

        PillText.Text = IsOn ? OnText : OffText;
    }
}
