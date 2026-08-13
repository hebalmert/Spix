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
    }

    public void Configure(string title, UserControl content)
    {
        _isCompleted = false;
        Result = ModalResult.Cancel();
        // Incluye los 24 px de margenes y los 56 px de padding del contenedor del modal.
        Width = Math.Max(720, content.MinWidth + 80);
        ModalTitleText.Text = title;
        ModalContent.Content = content;
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
