using System.Windows;

namespace Spix.AppWpf.SharedServices;

// El aspecto de la aplicacion en caliente: tema claro/oscuro y densidad de las tablas.
//
// Funciona porque los estilos piden los colores con DynamicResource: aqui se cambia el
// diccionario y la pantalla se repinta sola, sin reabrir nada.
public static class AppearanceService
{
    private const string TemaClaro = "Resources/Styles/ColorsLight.xaml";

    //Alto de fila de las dos densidades
    private const double FilaComoda = 48;
    private const double FilaCompacta = 36;
    private const double EncabezadoComodo = 42;
    private const double EncabezadoCompacto = 34;

    public static bool IsLight { get; private set; }

    public static bool IsCompact { get; private set; }

    // Pone el tema claro o vuelve al oscuro, que es el de casa
    public static void SetLightTheme(bool light)
    {
        IsLight = light;

        var diccionarios = Application.Current.Resources.MergedDictionaries;
        var actual = diccionarios.FirstOrDefault(x => x.Source != null &&
                                                      x.Source.OriginalString.EndsWith("ColorsLight.xaml", StringComparison.OrdinalIgnoreCase));

        if (!light)
        {
            if (actual != null)
            {
                diccionarios.Remove(actual);
            }

            return;
        }

        if (actual != null)
        {
            return;
        }

        //Se agrega de ultimo para que sus colores le ganen a los del tema oscuro
        diccionarios.Add(new ResourceDictionary
        {
            Source = new Uri(TemaClaro, UriKind.Relative)
        });
    }

    // Aprieta o suelta el alto de las filas de todos los listados
    public static void SetCompact(bool compact)
    {
        IsCompact = compact;

        Application.Current.Resources["IndexRowHeight"] = compact ? FilaCompacta : FilaComoda;
        Application.Current.Resources["IndexHeaderHeight"] = compact ? EncabezadoCompacto : EncabezadoComodo;
    }
}
