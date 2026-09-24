using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents.SharedButtonRow;

// El boton rojo de la fila: borrar. Es la unica accion que no se puede deshacer.
public partial class SharedButtonRowDelete : UserControl
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SharedButtonRowDelete));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SharedButtonRowDelete));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(FontAwesomeIcon), typeof(SharedButtonRowDelete),
            new PropertyMetadata(FontAwesomeIcon.TrashCan));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // El icono se puede cambiar; el color y la forma NO, esos los manda el estilo
    public FontAwesomeIcon Icon
    {
        get => (FontAwesomeIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public SharedButtonRowDelete()
    {
        InitializeComponent();
    }
}
