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

            if (Items.Count == 0)
            {
                Message = "No se encontraron registros.";
            }
        }
        catch (Exception exception)
        {
            Items.Clear();
            TotalPages = 0;
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
