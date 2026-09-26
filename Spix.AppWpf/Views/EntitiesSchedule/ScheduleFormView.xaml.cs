using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule;

// El formulario compartido por crear y editar una cita.
//
// Solo arma su propia vista: el DataContext se lo pone el modal que lo envuelve. Sin este
// archivo el control se monta VACIO, porque nadie llama a InitializeComponent.
public partial class ScheduleFormView : UserControl
{
    public ScheduleFormView()
    {
        InitializeComponent();
    }
}
