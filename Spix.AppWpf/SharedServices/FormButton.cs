using FontAwesome.Net.Generators;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.SharedServices;

// Lo que un boton de formulario necesita para pintarse y que no cabe en el Content:
// el icono que va dentro de la franja de color y el color de esa franja.
// Van como propiedades adjuntas para que el boton se siga usando con Style, sin cambiar
// la forma en que ya esta escrito en las vistas.
public static class FormButton
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.RegisterAttached("Icon", typeof(FontAwesomeIcon), typeof(FormButton),
            new PropertyMetadata(default(FontAwesomeIcon), IconChanged));

    // El estilo no puede preguntarle a un enum si viene vacio, asi que se deja escrito
    // aparte: se enciende en cuanto alguien pone un icono.
    private static readonly DependencyPropertyKey HasIconKey =
        DependencyProperty.RegisterAttachedReadOnly("HasIcon", typeof(bool), typeof(FormButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasIconProperty = HasIconKey.DependencyProperty;

    public static readonly DependencyProperty AccentProperty =
        DependencyProperty.RegisterAttached("Accent", typeof(Brush), typeof(FormButton),
            new PropertyMetadata(null));

    public static void SetIcon(DependencyObject elemento, FontAwesomeIcon valor)
    {
        elemento.SetValue(IconProperty, valor);
    }

    public static FontAwesomeIcon GetIcon(DependencyObject elemento)
    {
        return (FontAwesomeIcon)elemento.GetValue(IconProperty);
    }

    public static bool GetHasIcon(DependencyObject elemento)
    {
        return (bool)elemento.GetValue(HasIconProperty);
    }

    public static void SetAccent(DependencyObject elemento, Brush? valor)
    {
        elemento.SetValue(AccentProperty, valor);
    }

    public static Brush? GetAccent(DependencyObject elemento)
    {
        return (Brush?)elemento.GetValue(AccentProperty);
    }

    private static void IconChanged(DependencyObject elemento, DependencyPropertyChangedEventArgs e)
    {
        elemento.SetValue(HasIconKey, true);
    }
}
