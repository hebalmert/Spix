using FontAwesome.Net.Generators;
using FontAwesome.Net.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// Muestra un si o un no en una columna estrecha.
public partial class SharedFlag : UserControl
{
    public static readonly DependencyProperty IsOnProperty =
        DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(SharedFlag),
            new PropertyMetadata(false, EstadoCambio));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public SharedFlag()
    {
        InitializeComponent();
        Pintar();
    }

    private static void EstadoCambio(DependencyObject elemento, DependencyPropertyChangedEventArgs e)
    {
        ((SharedFlag)elemento).Pintar();
    }

    // El pincel se pide por nombre para que siga el tema si se cambia en caliente
    private void Pintar()
    {
        Glifo.Icon = IsOn ? FontAwesomeIcon.CircleCheck : FontAwesomeIcon.Minus;

        Glifo.SetResourceReference(
            FontAwesomeImage.ForegroundProperty,
            IsOn ? "BrushCatalogStockOk" : "BrushIndexMeta");
    }
}
