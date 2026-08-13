using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents;

// Muestra navegacion de paginas sin forzar que cada indice reimplemente sus botones.
public partial class SharedPagination : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
        nameof(CurrentPage),
        typeof(int),
        typeof(SharedPagination),
        new PropertyMetadata(1, OnPaginationChanged));

    public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register(
        nameof(TotalPages),
        typeof(int),
        typeof(SharedPagination),
        new PropertyMetadata(0, OnPaginationChanged));

    public static readonly DependencyProperty PageCommandProperty = DependencyProperty.Register(
        nameof(PageCommand),
        typeof(ICommand),
        typeof(SharedPagination));

    public ObservableCollection<PageLink> PageLinks { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public SharedPagination()
    {
        InitializeComponent();
        DataContext = this;
    }

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public ICommand? PageCommand
    {
        get => (ICommand?)GetValue(PageCommandProperty);
        set => SetValue(PageCommandProperty, value);
    }

    public bool CanGoPrevious => CurrentPage > 1;

    public bool CanGoNext => CurrentPage < TotalPages;

    public int PreviousPage => Math.Max(1, CurrentPage - 1);

    public int NextPage => Math.Min(TotalPages, CurrentPage + 1);

    // Regenera el rango cercano a la pagina actual cuando cambia el resultado de una consulta.
    private static void OnPaginationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((SharedPagination)dependencyObject).RefreshLinks();
    }

    private void RefreshLinks()
    {
        PageLinks.Clear();

        if (TotalPages < 1)
        {
            NotifyNavigationProperties();
            return;
        }

        var start = Math.Max(1, CurrentPage - 2);
        var end = Math.Min(TotalPages, CurrentPage + 2);

        for (var page = start; page <= end; page++)
        {
            PageLinks.Add(new PageLink
            {
                Number = page,
                IsCurrent = page == CurrentPage
            });
        }

        NotifyNavigationProperties();
    }

    private void NotifyNavigationProperties()
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(CanGoPrevious)));
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(CanGoNext)));
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(PreviousPage)));
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(NextPage)));
    }
}

// Representa un boton numerico del rango visible de paginacion.
public class PageLink
{
    public int Number { get; init; }

    public bool IsCurrent { get; init; }
}
