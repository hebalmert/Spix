using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Spix.AppWpf.SharedComponents.SharedCatalog;

// Uno de los cuatro indicadores de la cabecera del catalogo.
public partial class SharedCatalogKpi : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(SharedCatalogKpi));

    // Es object y no int porque algunos tableros muestran dinero, no un conteo.
    // Asi la pantalla decide el formato con StringFormat y el indicador no se entera.
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(SharedCatalogKpi));

    public static readonly DependencyProperty AccentProperty =
        DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(SharedCatalogKpi));

    public static readonly DependencyProperty ValueBrushProperty =
        DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(SharedCatalogKpi));

    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Brush? Accent
    {
        get => (Brush?)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    public Brush? ValueBrush
    {
        get => (Brush?)GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    public SharedCatalogKpi()
    {
        InitializeComponent();

        //Por defecto el numero va del azul del titulo; quien quiera otro lo pasa
        SetResourceReference(ValueBrushProperty, "BrushCatalogName");
        SetResourceReference(AccentProperty, "BrushCatalogKpiAccent");
    }
}
