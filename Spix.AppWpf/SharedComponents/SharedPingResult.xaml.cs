using Spix.xNetwork.PingHelper;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// El resultado de un ping. Lo usan los dos diagnosticos, el del servidor y el del nodo,
// igual que el PingResultView de la web.
public partial class SharedPingResult : UserControl
{
    public static readonly DependencyProperty ResultProperty =
        DependencyProperty.Register(nameof(Result), typeof(PingResult), typeof(SharedPingResult));

    public PingResult? Result
    {
        get => (PingResult?)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public SharedPingResult()
    {
        InitializeComponent();
    }
}
