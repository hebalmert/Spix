using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Spix.AppWpf.SharedComponents;

// Un paso del recorrido de la orden, o una de las cosas que faltan para cerrarla.
// Verde con su chulo cuando ya esta, gris mientras no.
public partial class SharedStepPill : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(SharedStepPill));

    public static readonly DependencyProperty IsDoneProperty = DependencyProperty.Register(
        nameof(IsDone), typeof(bool), typeof(SharedStepPill),
        new PropertyMetadata(false, Refrescar));

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsDone
    {
        get => (bool)GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }

    public SharedStepPill()
    {
        InitializeComponent();

        Pintar();
    }

    private static void Refrescar(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
    {
        ((SharedStepPill)objeto).Pintar();
    }

    private void Pintar()
    {
        Glifo.Icon = IsDone ? FontAwesomeIcon.CircleCheck : FontAwesomeIcon.Circle;

        var fondo = IsDone ? "BrushWhenDoneBack" : "BrushWhenNextBack";
        var letra = IsDone ? "BrushWhenDoneText" : "BrushWhenNextText";

        Marco.Background = Pincel(fondo);
        Marco.BorderBrush = Pincel(letra);
        Glifo.Foreground = Pincel(letra);
        Texto.Foreground = Pincel(letra);
    }

    // Los colores siguen viviendo en Colors.xaml: aqui solo se elige cual
    private static Brush Pincel(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}
