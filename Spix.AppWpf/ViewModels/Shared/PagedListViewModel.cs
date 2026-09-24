using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.Shared;

// Reutiliza busqueda y paginacion para cualquier listado que consume el patron Backend existente.
public abstract partial class PagedListViewModel<T> : ObservableObject
{
    private const int DefaultPageSize = 15;

    private readonly IPagedEntityService<T> _pagedEntityService;

    [ObservableProperty]
    private ObservableCollection<T> _items = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private int _totalRecords;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    protected abstract string Endpoint { get; }

    protected PagedListViewModel(IPagedEntityService<T> pagedEntityService)
    {
        _pagedEntityService = pagedEntityService;
    }

    // Actualiza la visibilidad del aviso cuando la consulta informa un resultado o error.
    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    // Carga la primera pagina cuando el usuario aplica un nuevo termino de busqueda.
    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(1);
    }

    // Restablece el filtro y solicita nuevamente la primera pagina.
    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(1);
    }

    // Vuelve a pedir la pagina que se esta viendo, sin perder el filtro ni la posicion.
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync(CurrentPage);
    }

    // Cambia de pagina sin descargar registros innecesarios.
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        await LoadAsync(page);
    }

    // No hace nada por defecto: solo lo usa quien lo necesite.
    protected virtual Task AfterLoadAsync()
    {
        return Task.CompletedTask;
    }

    public async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        Message = string.Empty;

        try
        {
            var result = await _pagedEntityService.GetPageAsync(
                Endpoint,
                page,
                DefaultPageSize,
                Filter);

            Items = new ObservableCollection<T>(result.Items);
            CurrentPage = page;
            TotalPages = result.TotalPages;
            TotalRecords = result.TotalRecords;

            //Cuando no hay filas no se pone mensaje: de eso se encarga el aviso de la
            //tabla vacia. El mensaje queda solo para los errores.

            //Gancho para las pantallas que tienen que hacer algo despues de cada carga,
            //como el catalogo, que vuelve a elegir la categoria y baja sus hijos.
            await AfterLoadAsync();
        }
        catch (Exception exception)
        {
            //Se reemplaza la lista, no se vacia: asi la pantalla se entera del cambio
            Items = new ObservableCollection<T>();
            TotalPages = 0;
            TotalRecords = 0;
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
