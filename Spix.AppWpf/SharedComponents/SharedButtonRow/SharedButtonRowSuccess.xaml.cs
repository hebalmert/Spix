using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents.SharedButtonRow;

// El boton verde de la fila: la accion propia del modulo, como enviar el correo de prueba.
public partial class SharedButtonRowSuccess : UserControl
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SharedButtonRowSuccess));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SharedButtonRowSuccess));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(FontAwesomeIcon), typeof(SharedButtonRowSuccess),
            new PropertyMetadata(FontAwesomeIcon.PaperPlane));

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

    public SharedButtonRowSuccess()
    {
        InitializeComponent();
    }
}
