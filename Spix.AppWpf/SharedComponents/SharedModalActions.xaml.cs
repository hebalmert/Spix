using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents;

// El pie de TODOS los modales: Volver y Guardar, del ancho de su texto y a la derecha.
//
// Se ajusta por propiedades para que ningun formulario tenga que armar sus botones a mano:
// se le cambian los textos y los iconos, se le esconde el de guardar cuando el modal solo
// consulta, y se le mete un tercer boton por Extra cuando hace falta (Eliminar, Reenviar
// correo). Armarlos a mano es lo que hacia que unos modales se vieran distintos a otros.
public partial class SharedModalActions : UserControl
{
    public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register(
        nameof(SaveCommand), typeof(ICommand), typeof(SharedModalActions));

    public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
        nameof(CancelCommand), typeof(ICommand), typeof(SharedModalActions));

    public static readonly DependencyProperty SaveTextProperty = DependencyProperty.Register(
        nameof(SaveText), typeof(string), typeof(SharedModalActions),
        new PropertyMetadata("Guardar"));

    public static readonly DependencyProperty CancelTextProperty = DependencyProperty.Register(
        nameof(CancelText), typeof(string), typeof(SharedModalActions),
        new PropertyMetadata("Volver"));

    public static readonly DependencyProperty SaveIconProperty = DependencyProperty.Register(
        nameof(SaveIcon), typeof(FontAwesomeIcon), typeof(SharedModalActions),
        new PropertyMetadata(FontAwesomeIcon.FloppyDisk));

    public static readonly DependencyProperty CancelIconProperty = DependencyProperty.Register(
        nameof(CancelIcon), typeof(FontAwesomeIcon), typeof(SharedModalActions),
        new PropertyMetadata(FontAwesomeIcon.ArrowLeft));

    // Un modal que solo consulta deja el de guardar escondido y se queda con Volver
    public static readonly DependencyProperty ShowSaveProperty = DependencyProperty.Register(
        nameof(ShowSave), typeof(bool), typeof(SharedModalActions),
        new PropertyMetadata(true));

    // Sitio para un tercer boton propio del modal; sale a la izquierda de la pareja
    public static readonly DependencyProperty ExtraProperty = DependencyProperty.Register(
        nameof(Extra), typeof(object), typeof(SharedModalActions));

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public string? SaveText
    {
        get => (string?)GetValue(SaveTextProperty);
        set => SetValue(SaveTextProperty, value);
    }

    public string? CancelText
    {
        get => (string?)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public FontAwesomeIcon SaveIcon
    {
        get => (FontAwesomeIcon)GetValue(SaveIconProperty);
        set => SetValue(SaveIconProperty, value);
    }

    public FontAwesomeIcon CancelIcon
    {
        get => (FontAwesomeIcon)GetValue(CancelIconProperty);
        set => SetValue(CancelIconProperty, value);
    }

    public bool ShowSave
    {
        get => (bool)GetValue(ShowSaveProperty);
        set => SetValue(ShowSaveProperty, value);
    }

    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public SharedModalActions()
    {
        InitializeComponent();
    }
}
