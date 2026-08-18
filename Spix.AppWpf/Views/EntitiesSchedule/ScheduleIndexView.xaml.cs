using Spix.AppWpf.SharedComponents.SharedCalendar;
using Spix.AppWpf.ViewModels.EntitiesSchedule;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule;

// Vincula los clics del calendario embebido con las acciones del modulo Schedule.
public partial class ScheduleIndexView : UserControl
{
    private readonly ScheduleIndexViewModel _viewModel;
    private bool _loaded;

    public ScheduleIndexView(ScheduleIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.EventsChanged += UpdateCalendar;
        CalendarView.DateClicked += OpenCreate;
        CalendarView.EventClicked += OpenEvent;
        Loaded += LoadView;
        Unloaded += UnloadView;
    }

    // Recupera el rango de eventos usado por la pagina Schedule de Blazor.
    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.LoadAsync();
    }

    // Reemplaza los eventos presentados sin reconstruir la vista principal.
    private async void UpdateCalendar(object? sender, EventArgs e)
    {
        await CalendarView.SetEventsAsync(_viewModel.Events);
    }

    // Abre la agenda nueva con la fecha seleccionada por el usuario.
    private async void OpenCreate(object? sender, CalendarDateClickEventArgs e)
    {
        await _viewModel.OpenCreateAsync(e.Date);
    }

    // Abre edicion o detalle segun el origen del evento seleccionado.
    private async void OpenEvent(object? sender, CalendarEventClickEventArgs e)
    {
        await _viewModel.OpenEventAsync(e.EventId);
    }

    // Libera los eventos de la vista cuando el contenedor principal cambia de modulo.
    private void UnloadView(object sender, RoutedEventArgs e)
    {
        _viewModel.EventsChanged -= UpdateCalendar;
        CalendarView.DateClicked -= OpenCreate;
        CalendarView.EventClicked -= OpenEvent;
        Unloaded -= UnloadView;
    }
}
