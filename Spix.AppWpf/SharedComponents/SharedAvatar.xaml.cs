using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spix.AppWpf.SharedComponents;

// La foto de un registro, con un icono de respaldo cuando no hay foto.
//
// Va en un solo nivel, como el resto de los Shared: un marco redondo con la imagen dentro
// y el icono encima. No se arma anidando otros componentes.
public partial class SharedAvatar : UserControl
{
    public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
        nameof(Url), typeof(string), typeof(SharedAvatar),
        new PropertyMetadata(null, Refrescar));

    // El icono que se ve cuando no hay foto: persona para un cliente, camion para un proveedor
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(FontAwesomeIcon), typeof(SharedAvatar),
        new PropertyMetadata(FontAwesomeIcon.User, Refrescar));

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(SharedAvatar),
        new PropertyMetadata(34d, Refrescar));

    // Una imagen ya cargada, para cuando no hay direccion que bajar: es lo que usa el
    // formulario al elegir una foto del disco, que todavia no esta en el servidor.
    // Si viene, manda sobre Url.
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(ImageSource), typeof(SharedAvatar),
        new PropertyMetadata(null, Refrescar));

    public string? Url
    {
        get => (string?)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public FontAwesomeIcon Icon
    {
        get => (FontAwesomeIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public SharedAvatar()
    {
        InitializeComponent();

        //Si la direccion existe pero la imagen no se puede bajar, se cae al icono
        Foto.ImageFailed += (_, _) => SinFoto();

        Pintar();
    }

    private static void Refrescar(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
    {
        ((SharedAvatar)objeto).Pintar();
    }

    private void Pintar()
    {
        Medidas();

        Glifo.Icon = Icon;

        //Una imagen ya cargada manda sobre la direccion: es la foto que se acaba de elegir
        if (Source is not null)
        {
            Foto.Source = Source;
            ConFoto();
            return;
        }

        //El backend manda la direccion de "NoImage" cuando el registro no tiene foto: eso
        //cuenta como sin foto, asi se ve el icono y no el dibujo de la camara tachada.
        var hayFoto =
            !string.IsNullOrWhiteSpace(Url) &&
            Url!.IndexOf("NoImage", StringComparison.OrdinalIgnoreCase) < 0 &&
            Uri.TryCreate(Url, UriKind.Absolute, out var direccion);

        if (!hayFoto)
        {
            SinFoto();
            return;
        }

        try
        {
            //OnLoad para que el archivo no quede tomado y la fila se pueda refrescar
            var imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.UriSource = new Uri(Url!, UriKind.Absolute);
            imagen.CacheOption = BitmapCacheOption.OnLoad;
            imagen.DecodePixelWidth = (int)(Size * 2);
            imagen.EndInit();

            Foto.Source = imagen;
            ConFoto();
        }
        catch (Exception)
        {
            //Una direccion mal formada no puede tumbar el listado entero
            SinFoto();
        }
    }

    private void ConFoto()
    {
        Foto.Visibility = Visibility.Visible;
        Glifo.Visibility = Visibility.Collapsed;

        Marco.Background = (Brush)FindResource("BrushAvatarPhotoBack");
        Marco.BorderBrush = (Brush)FindResource("BrushAvatarPhotoBack");
    }

    private void SinFoto()
    {
        Foto.Source = null;
        Foto.Visibility = Visibility.Collapsed;
        Glifo.Visibility = Visibility.Visible;

        Marco.Background = (Brush)FindResource("BrushAvatarBack");
        Marco.BorderBrush = (Brush)FindResource("BrushAvatarBorder");
    }

    // El marco es redondo: el radio es la mitad del lado
    private void Medidas()
    {
        Marco.Width = Size;
        Marco.Height = Size;
        Marco.CornerRadius = new CornerRadius(Size / 2);
        Marco.BorderThickness = new Thickness(2);

        Glifo.Width = Size / 2;
        Glifo.Height = Size / 2;
    }
}
