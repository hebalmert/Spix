using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.Shared;

// El maestro-detalle del catalogo, igual que en Blazor: las categorias se piden paginadas
// a la izquierda y, al elegir una, sus hijos se bajan enteros a la derecha.
//
// Aqui vive TODO lo que los cuatro modulos (Planes, Servicios, Productos, Marcas) hacen
// igual. Cada modulo solo dice cual es su endpoint hijo, como se saca el id de su
// categoria y que hace cada chip.
public abstract partial class CategoryDetailViewModel<TCategory, TChild> : PagedListViewModel<TCategory>
    where TCategory : class
    where TChild : class
{
    public const string ChipAll = "all";

    // Los hijos no se paginan: se bajan de una sola vez, igual que en la web
    protected const int ChildPageSize = 100;

    // Mientras se restaura la seleccion despues de recargar, el cambio no dispara otra
    // bajada de hijos: de eso se encarga AfterLoadAsync.
    private bool _restaurando;

    // El id de la categoria elegida se guarda aparte porque la lista se REEMPLAZA en cada
    // carga: al cambiar el ItemsSource, el ListBox pone la seleccion en nulo antes de que
    // podamos restaurarla. Si nos fiaramos del objeto, la pantalla saltaria siempre a la
    // primera categoria despues de guardar o de cambiar de pagina.
    private Guid? _selectedCategoryId;

    private TCategory? _selectedCategory;

    public TCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (ReferenceEquals(_selectedCategory, value))
            {
                return;
            }

            _selectedCategory = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectedCategoryName));

            //Nulo es el ListBox soltando la seleccion porque le cambiaron la lista:
            //no se toca el id guardado ni se pide nada.
            if (value is null)
            {
                return;
            }

            _selectedCategoryId = GetCategoryId(value);

            if (_restaurando)
            {
                return;
            }

            //Al cambiar de categoria se arranca limpio, como en la web
            Chip = ChipAll;
            ChildFilter = string.Empty;

            _ = LoadChildrenAsync();
        }
    }

    public ObservableCollection<TChild> Children { get; } = new();

    public ObservableCollection<TChild> FilteredChildren { get; } = new();

    private bool _isChildrenLoading;

    public bool IsChildrenLoading
    {
        get => _isChildrenLoading;
        private set
        {
            if (_isChildrenLoading == value)
            {
                return;
            }

            _isChildrenLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowChildrenEmpty));
        }
    }

    private string _childFilter = string.Empty;

    // El texto de la derecha va al SERVIDOR: asi busca en todos los hijos de la categoria,
    // no solo en los que estan en pantalla. Igual que en Blazor.
    public string ChildFilter
    {
        get => _childFilter;
        set
        {
            if (_childFilter == value)
            {
                return;
            }

            _childFilter = value;
            OnPropertyChanged();
        }
    }

    private string _chip = ChipAll;

    public string Chip
    {
        get => _chip;
        private set
        {
            if (_chip == value)
            {
                return;
            }

            _chip = value;
            OnPropertyChanged();
            RefreshFilteredChildren();
        }
    }

    public bool HasSelection => SelectedCategory is not null;

    public bool ShowChildrenEmpty => HasSelection && !IsChildrenLoading && FilteredChildren.Count == 0;

    // Los dos primeros indicadores son iguales en los cuatro modulos
    public int CategoriesCount => Items.Count;

    public int ChildrenCount => Children.Count;

    public int FilteredChildrenCount => FilteredChildren.Count;

    // Lo que cada modulo tiene que decir de si mismo
    protected abstract string ChildEndpoint { get; }

    protected abstract Guid GetCategoryId(TCategory category);

    public abstract string SelectedCategoryName { get; }

    // El chip filtra en memoria, sobre lo que ya se bajo
    protected abstract IEnumerable<TChild> ApplyChip(IEnumerable<TChild> children, string chip);

    // Cada modulo avisa de sus dos indicadores propios
    protected abstract void NotifyKpis();

    // Lo resuelve cada modulo con su propio IRepository ya inyectado
    protected abstract Task<IReadOnlyCollection<TChild>> GetChildrenAsync(string url);

    protected CategoryDetailViewModel(IPagedEntityService<TCategory> pagedEntityService)
        : base(pagedEntityService)
    {
    }

    [RelayCommand]
    private void SetChip(string? chip)
    {
        Chip = string.IsNullOrWhiteSpace(chip) ? ChipAll : chip;
    }

    // Buscar dentro de los hijos vuelve a pedirlos al Backend con el filtro puesto
    [RelayCommand]
    private async Task SearchChildrenAsync()
    {
        await LoadChildrenAsync();
    }

    [RelayCommand]
    private async Task ClearChildSearchAsync()
    {
        ChildFilter = string.Empty;
        await LoadChildrenAsync();
    }

    // Despues de cada carga de categorias se conserva la elegida; si ya no esta, se toma
    // la primera. Es lo mismo que hace Cargar() en Blazor.
    protected override async Task AfterLoadAsync()
    {
        OnPropertyChanged(nameof(CategoriesCount));

        var nueva = _selectedCategoryId is not null
            ? Items.FirstOrDefault(item => GetCategoryId(item) == _selectedCategoryId.Value)
            : null;

        //Si la que estaba ya no esta (se borro, o el filtro la dejo fuera) se toma la primera
        nueva ??= Items.FirstOrDefault();

        _restaurando = true;
        SelectedCategory = nueva;
        _restaurando = false;

        await LoadChildrenAsync();
    }

    // Vuelve a cargar el padre dejando elegida la categoria que se acaba de tocar, porque
    // el contador de hijos de la fila cambia.
    protected async Task ReloadKeepingSelectionAsync(Guid categoryId)
    {
        _selectedCategoryId = categoryId;

        await LoadAsync(CurrentPage);
    }

    protected async Task LoadChildrenAsync()
    {
        if (SelectedCategory is null)
        {
            ReplaceChildren(Array.Empty<TChild>());
            return;
        }

        var categoryId = GetCategoryId(SelectedCategory);

        IsChildrenLoading = true;

        try
        {
            var url = $"{ChildEndpoint}?guidId={categoryId}&page=1&recordsnumber={ChildPageSize}";

            if (!string.IsNullOrWhiteSpace(ChildFilter))
            {
                url += $"&filter={Uri.EscapeDataString(ChildFilter.Trim())}";
            }

            ReplaceChildren(await GetChildrenAsync(url));
        }
        catch (Exception exception)
        {
            ReplaceChildren(Array.Empty<TChild>());
            Message = exception.Message;
        }
        finally
        {
            IsChildrenLoading = false;
        }
    }

    private void ReplaceChildren(IEnumerable<TChild> children)
    {
        Children.Clear();

        foreach (var child in children)
        {
            Children.Add(child);
        }

        OnPropertyChanged(nameof(ChildrenCount));
        NotifyKpis();
        RefreshFilteredChildren();
    }

    private void RefreshFilteredChildren()
    {
        FilteredChildren.Clear();

        foreach (var child in ApplyChip(Children, Chip))
        {
            FilteredChildren.Add(child);
        }

        OnPropertyChanged(nameof(FilteredChildrenCount));
        OnPropertyChanged(nameof(ShowChildrenEmpty));
    }
}
