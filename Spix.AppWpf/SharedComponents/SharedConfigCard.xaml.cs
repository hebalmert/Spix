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

    // Si la pieza, ESTANDO puesta, le esta dando servicio al cliente.
    // null = no aplica (la mayoria de las tarjetas): la barra se comporta como siempre.
    // false = armada pero sin servicio -> barra ROJA.
    public static readonly DependencyProperty HasAccessProperty = DependencyProperty.Register(
        nameof(HasAccess), typeof(bool?), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

    // Los datos de la pieza, cada uno con su rotulo: lo mismo que la web pinta en cc-facts.
    // Es opcional: la tarjeta que no los manda se ve como siempre.
    public static readonly DependencyProperty FactsProperty = DependencyProperty.Register(
        nameof(Facts), typeof(IEnumerable<ConfigCardFact>), typeof(SharedConfigCard),
        new PropertyMetadata(null, Refrescar));

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

    public bool? HasAccess
    {
        get => (bool?)GetValue(HasAccessProperty);
        set => SetValue(HasAccessProperty, value);
    }

    public IEnumerable<ConfigCardFact>? Facts
    {
        get => (IEnumerable<ConfigCardFact>?)GetValue(FactsProperty);
        set => SetValue(FactsProperty, value);
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

        //La barra lateral tiene TRES estados, no dos: ambar si falta armar la pieza,
        //verde si esta armada y el cliente navega, y ROJA si esta armada pero sin
        //servicio (cortado por mora o bloqueado a mano).
        var sinServicio = IsDone && HasAccess == false;

        Barra.Background = Pincel(!IsDone ? "BrushCatalogKpiWarn"
            : sinServicio ? "BrushCatalogKpiBad" : "BrushCatalogKpiOk");

        Estado.Background = Pincel(IsDone ? "BrushWhenDoneBack" : "BrushWhenLateBack");
        EstadoTexto.Foreground = Pincel(IsDone ? "BrushWhenDoneText" : "BrushWhenLateText");
        EstadoTexto.Text = IsDone ? "Configurado" : "Falta";

        //Sin pieza se dice que falta, en vez de dejar el hueco vacio
        Dato.Text = IsDone ? Value : EmptyText;

        //Los datos solo cuando la pieza esta puesta y la pantalla los mando. Con datos, la
        //linea de arriba sobra: ya estan ahi abajo con su rotulo.
        var hayDatos = IsDone && Facts is not null && Facts.Any();
        Datos.Visibility = Ver(hayDatos);
        Dato.Visibility = Ver(!hayDatos);

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

// Un dato de la tarjeta: el rotulo y lo que dice. Es lo que la web pinta como cc-fact.
public sealed class ConfigCardFact
{
    public string Label { get; }

    public string? Value { get; }

    //Para el dato que avisa de un problema (el Acceso en OFF): se pinta en rojo
    public bool IsAlert { get; }

    public ConfigCardFact(string label, string? value, bool isAlert = false)
    {
        Label = label;
        Value = value;
        IsAlert = isAlert;
    }
}
