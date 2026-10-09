using System.Windows.Input;

namespace Spix.AppMaui.SharedComponents;

//Un solo boton para toda la app, por parametros. Las variantes de la web (azul, verde,
//gris, rojo, morado) son solo otro color de franja: no hay un componente por color.
public partial class SpixButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(SpixButton), string.Empty);

    //El simbolo de la franja. Va en Unicode porque la app todavia no tiene la fuente de
    //iconos de la web; cuando se agregue el ttf, aqui se cambia el glifo y nada mas.
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(SpixButton), string.Empty);

    public static readonly BindableProperty StripProperty =
        BindableProperty.Create(nameof(Strip), typeof(Color), typeof(SpixButton), Colors.SteelBlue);

    public static readonly BindableProperty TextColorValueProperty =
        BindableProperty.Create(nameof(TextColorValue), typeof(Color), typeof(SpixButton), Colors.Black);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SpixButton));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SpixButton));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public Color Strip
    {
        get => (Color)GetValue(StripProperty);
        set => SetValue(StripProperty, value);
    }

    public Color TextColorValue
    {
        get => (Color)GetValue(TextColorValueProperty);
        set => SetValue(TextColorValueProperty, value);
    }

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

    public SpixButton()
    {
        InitializeComponent();
    }
}
