using Spix.AppWpf.SharedServices;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// Aloja formularios reutilizables con un encabezado y cierre consistente en toda la aplicacion.
public partial class SharedModalWindow : Window
{
    private bool _isCompleted;

    public ModalResult Result { get; private set; } = ModalResult.Cancel();

    public SharedModalWindow()
    {
        InitializeComponent();

        //CenterOwner centra la ventana con el tamano que tiene al mostrarse, pero como el
        //alto sale del contenido (SizeToContent) todavia no se conoce: la ventana queda
        //corrida hacia abajo, y cuanto mas alto el formulario, mas se corre. Por eso se
        //vuelve a centrar cuando ya se sabe cuanto mide.
        SizeChanged += (_, _) => Centrar();
    }

    // Deja el modal en el centro de la ventana principal, sin salirse de la pantalla.
    private void Centrar()
    {
        if (Owner is null)
        {
            return;
        }

        //Con la ventana principal maximizada sus Left y Top son los de antes de maximizar,
        //no los de la pantalla: ahi se centra contra el area de trabajo.
        var area = Owner.WindowState == WindowState.Maximized
            ? new Rect(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top,
                       SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height)
            : new Rect(Owner.Left, Owner.Top, Owner.Width, Owner.Height);

        var izquierda = area.Left + ((area.Width - ActualWidth) / 2);
        var arriba = area.Top + ((area.Height - ActualHeight) / 2);

        //Un formulario mas alto que la pantalla no puede empezar por encima del borde
        Left = Math.Max(SystemParameters.WorkArea.Left, izquierda);
        Top = Math.Max(SystemParameters.WorkArea.Top, arriba);
    }

    public void Configure(string title, UserControl content)
    {
        _isCompleted = false;
        Result = ModalResult.Cancel();

        //El ancho lo pide el formulario, no el modal. Se suman los 24 px de margenes, los
        //56 del relleno y los 10 de la barra de desplazamiento, y nunca se pasa de la
        //pantalla.
        var pedido = AnchoPedido(content) + 90;
        var tope = SystemParameters.WorkArea.Width - 60;

        Width = Math.Min(Math.Max(720, pedido), tope);

        ModalTitleText.Text = title;
        ModalContent.Content = content;
    }

    // Busca el MinWidth mas grande del contenido y de lo que lleva dentro.
    //
    // Hace falta mirar hacia adentro porque el MinWidth se declara en el FormView, y al
    // modal le llega el DialogView que lo envuelve: si solo se mirara el de afuera, todos
    // los formularios anchos se quedarian en los 720 de siempre y sus campos saldrian
    // aplastados.
    private static double AnchoPedido(DependencyObject elemento, int profundidad = 0)
    {
        if (profundidad > 6)
        {
            return 0;
        }

        var mayor = elemento is FrameworkElement marco && !double.IsInfinity(marco.MinWidth)
            ? marco.MinWidth
            : 0;

        foreach (var hijo in LogicalTreeHelper.GetChildren(elemento))
        {
            if (hijo is DependencyObject dependiente)
            {
                mayor = Math.Max(mayor, AnchoPedido(dependiente, profundidad + 1));
            }
        }

        return mayor;
    }

    // Cierra el modal con el resultado que necesita el indice que lo abrio.
    public void Complete(ModalResult result)
    {
        _isCompleted = true;
        Result = result;
        DialogResult = result.Succeeded;
        Close();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Complete(ModalResult.Cancel());
    }

    // Garantiza Cancel cuando el usuario cierra el dialogo desde el sistema operativo.
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isCompleted)
        {
            Result = ModalResult.Cancel();
        }

        base.OnClosing(e);
    }
}
