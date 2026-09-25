using FontAwesome.Net.Generators;
using FontAwesome.Net.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedCatalog;

// El aviso de resultado de un diagnostico, con la misma cara de los indicadores.
public partial class SharedCatalogStatusCard : UserControl
{
    public static readonly DependencyProperty IsOkProperty =
        DependencyProperty.Register(nameof(IsOk), typeof(bool), typeof(SharedCatalogStatusCard),
            new PropertyMetadata(false, EstadoCambio));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SharedCatalogStatusCard));

    public static readonly DependencyProperty DetailProperty =
        DependencyProperty.Register(nameof(Detail), typeof(string), typeof(SharedCatalogStatusCard));

    public bool IsOk
    {
        get => (bool)GetValue(IsOkProperty);
        set => SetValue(IsOkProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Detail
    {
        get => (string?)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public SharedCatalogStatusCard()
    {
        InitializeComponent();
        Pintar();
    }

    private static void EstadoCambio(DependencyObject elemento, DependencyPropertyChangedEventArgs e)
    {
        ((SharedCatalogStatusCard)elemento).Pintar();
    }

    // Los pinceles se piden por nombre para que sigan el tema si se cambia en caliente
    private void Pintar()
    {
        AccentBar.SetResourceReference(
            System.Windows.Controls.Border.BackgroundProperty,
            IsOk ? "BrushCatalogKpiOk" : "BrushCatalogKpiBad");

        Glifo.Icon = IsOk ? FontAwesomeIcon.CircleCheck : FontAwesomeIcon.TriangleExclamation;

        Glifo.SetResourceReference(
            FontAwesomeImage.ForegroundProperty,
            IsOk ? "BrushCatalogStockOk" : "BrushCatalogStockNone");
    }
}
