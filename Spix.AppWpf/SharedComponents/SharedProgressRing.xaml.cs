using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Spix.AppWpf.SharedComponents;

// El avance de una configuracion, dibujado como anillo.
//
// Es el mismo anillo de la web (progress-ring): se ve de un vistazo cuanto falta, sin
// tener que leer un numero suelto. WPF no trae un control de progreso circular, asi que
// el arco se arma aqui con geometria.
public partial class SharedProgressRing : UserControl
{
    //El radio del aro, contando que el trazo es de 12 y el lienzo de 132
    private const double Radio = 60;

    private const double Centro = 66;

    public static readonly DependencyProperty DoneProperty = DependencyProperty.Register(
        nameof(Done), typeof(int), typeof(SharedProgressRing),
        new PropertyMetadata(0, Refrescar));

    public static readonly DependencyProperty TotalProperty = DependencyProperty.Register(
        nameof(Total), typeof(int), typeof(SharedProgressRing),
        new PropertyMetadata(0, Refrescar));

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        nameof(Caption), typeof(string), typeof(SharedProgressRing));

    public int Done
    {
        get => (int)GetValue(DoneProperty);
        set => SetValue(DoneProperty, value);
    }

    public int Total
    {
        get => (int)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    public string? Caption
    {
        get => (string?)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public SharedProgressRing()
    {
        InitializeComponent();

        Pintar();
    }

    private static void Refrescar(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
    {
        ((SharedProgressRing)objeto).Pintar();
    }

    private void Pintar()
    {
        var total = Total <= 0 ? 1 : Total;
        var hechos = Math.Clamp(Done, 0, total);
        var porcion = (double)hechos / total;

        Numero.Text = $"{hechos}/{total}";
        Porcentaje.Text = $"{(int)(porcion * 100)}%";

        //Verde cuando ya esta todo; mientras falte algo, ambar
        var color = hechos >= total ? "BrushCatalogKpiOk" : "BrushCatalogKpiWarn";
        Arco.Stroke = Pincel(color);

        Arco.Data = ArcoDe(porcion);
    }

    // El arco desde las 12 en punto, en el sentido del reloj
    private static Geometry? ArcoDe(double porcion)
    {
        if (porcion <= 0)
        {
            return null;
        }

        //Un circulo entero no se puede dibujar con un solo arco: se cierra con dos mitades
        if (porcion >= 1)
        {
            return new EllipseGeometry(new Point(Centro, Centro), Radio, Radio);
        }

        var inicio = new Point(Centro, Centro - Radio);
        var angulo = porcion * 2 * Math.PI;

        var fin = new Point(
            Centro + (Radio * Math.Sin(angulo)),
            Centro - (Radio * Math.Cos(angulo)));

        var figura = new PathFigure { StartPoint = inicio };

        figura.Segments.Add(new ArcSegment
        {
            Point = fin,
            Size = new Size(Radio, Radio),
            SweepDirection = SweepDirection.Clockwise,
            //Pasada la mitad hay que decirle que tome el camino largo
            IsLargeArc = porcion > 0.5
        });

        var geometria = new PathGeometry();
        geometria.Figures.Add(figura);

        return geometria;
    }

    private static Brush Pincel(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}
