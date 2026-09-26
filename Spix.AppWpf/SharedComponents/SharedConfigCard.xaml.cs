using FontAwesome.Net.Generators;
using Spix.AppWpf.SharedServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Spix.AppWpf.SharedComponents;

// Una pieza de la configuracion del servicio de un contrato.
//
// La regla que manda, y que viene de la web: una pieza NO SE CAMBIA. Se quita y se vuelve
// a agregar. Por eso agregar solo aparece mientras falta, y quitar solo cuando ya esta.
//
// Y si el contrato ya tiene Queue o IpBinding, quitar queda BLOQUEADO: esas piezas ya
// estan escritas en el MikroTik. El boton no se esconde, se apaga y cambia su icono por un
// candado, con el globo explicando que hay que quitar primero. Si desapareciera, el usuario
// no entenderia por que no puede.
public partial class SharedConfigCard : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SharedConfigCard));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    // Lo que se lee cuando la pieza todavia falta
    public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(
        nameof(EmptyText), typeof(string), typeof(SharedConfigCard),
        new PropertyMetadata("Sin asignar", Refrescar));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(FontAwesomeIcon), typeof(SharedConfigCard),
        new PropertyMetadata(FontAwesomeIcon.Gear, Refrescar));

    public static readonly DependencyProperty IsDoneProperty = DependencyProperty.Register(
        nameof(IsDone), typeof(bool), typeof(SharedConfigCard),
        new PropertyMetadata(false, Refrescar));

    // Si se puede quitar; con false el boton se apaga y sale el candado
    public static readonly DependencyProperty CanRemoveProperty = DependencyProperty.Register(
        nameof(CanRemove), typeof(bool), typeof(SharedConfigCard),
        new PropertyMetadata(true, Refrescar));

    public static readonly DependencyProperty LockTipProperty = DependencyProperty.Register(
        nameof(LockTip), typeof(string), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    // Lo que dice el globo del boton de agregar ("Agregar", "Crear la queue"...)
    public static readonly DependencyProperty AddTipProperty = DependencyProperty.Register(
        nameof(AddTip), typeof(string), typeof(SharedConfigCard),
        new PropertyMetadata("Agregar", Refrescar));

    public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
        nameof(AddCommand), typeof(ICommand), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
        nameof(RemoveCommand), typeof(ICommand), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    // Opcionales: si no se les da comando, no aparecen
    public static readonly DependencyProperty ViewCommandProperty = DependencyProperty.Register(
        nameof(ViewCommand), typeof(ICommand), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
        nameof(EditCommand), typeof(ICommand), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? EmptyText
    {
        get => (string?)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public FontAwesomeIcon Icon
    {
        get => (FontAwesomeIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsDone
    {
        get => (bool)GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }

    public bool CanRemove
    {
        get => (bool)GetValue(CanRemoveProperty);
        set => SetValue(CanRemoveProperty, value);
    }

    public string? LockTip
    {
        get => (string?)GetValue(LockTipProperty);
        set => SetValue(LockTipProperty, value);
    }

    public string? AddTip
    {
        get => (string?)GetValue(AddTipProperty);
        set => SetValue(AddTipProperty, value);
    }

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public ICommand? RemoveCommand
    {
        get => (ICommand?)GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public ICommand? ViewCommand
    {
        get => (ICommand?)GetValue(ViewCommandProperty);
        set => SetValue(ViewCommandProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public SharedConfigCard()
    {
        InitializeComponent();

        Pintar();
    }

    private static void Refrescar(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
    {
        ((SharedConfigCard)objeto).Pintar();
    }

    private void Pintar()
    {
        Glifo.Icon = Icon;

        //La barra lateral y la pastilla dicen de un vistazo si la pieza esta o falta
        Barra.Background = Pincel(IsDone ? "BrushCatalogKpiOk" : "BrushCatalogKpiWarn");

        Estado.Background = Pincel(IsDone ? "BrushWhenDoneBack" : "BrushWhenLateBack");
        EstadoTexto.Foreground = Pincel(IsDone ? "BrushWhenDoneText" : "BrushWhenLateText");
        EstadoTexto.Text = IsDone ? "Configurado" : "Falta";

        //Sin pieza se dice que falta, en vez de dejar el hueco vacio
        Dato.Text = IsDone ? Value : EmptyText;

        //Agregar SOLO mientras falta: una pieza puesta no se cambia, se quita y se agrega
        BotonAgregar.Visibility = Ver(!IsDone && AddCommand is not null);
        BotonAgregar.ToolTip = AddTip;

        //Los dos opcionales, solo con la pieza puesta
        BotonVer.Visibility = Ver(IsDone && ViewCommand is not null);
        BotonVer.ToolTip = "Ver en el mapa";

        BotonEditar.Visibility = Ver(IsDone && EditCommand is not null);
        BotonEditar.ToolTip = "Editar";

        //Quitar solo con la pieza puesta; bloqueado se apaga y cambia al candado
        BotonQuitar.Visibility = Ver(IsDone && RemoveCommand is not null);
        BotonQuitar.IsEnabled = CanRemove;
        BotonQuitar.ToolTip = CanRemove ? "Quitar" : LockTip;

        FormButton.SetIcon(BotonQuitar, CanRemove ? FontAwesomeIcon.TrashCan : FontAwesomeIcon.Lock);
    }

    private static Visibility Ver(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private static Brush Pincel(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}
