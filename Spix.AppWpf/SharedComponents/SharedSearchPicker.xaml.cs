using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents;

// Se escribe y debajo caen los resultados; al elegir uno se cierra la lista.
//
// El componente no sabe que se esta buscando: avisa de cada tecla con SearchCommand y
// entrega el elegido con SelectCommand. La regla de "desde cuantas letras se busca" vive
// en el ViewModel, igual que en la web.
public partial class SharedSearchPicker : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(SharedSearchPicker),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(IEnumerable), typeof(SharedSearchPicker),
        new PropertyMetadata(null, ItemsCambio));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SharedSearchPicker));

    // Se dispara con cada tecla, con el texto escrito
    public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register(
        nameof(SearchCommand), typeof(ICommand), typeof(SharedSearchPicker));

    // Se dispara al elegir, con el elemento elegido
    public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
        nameof(SelectCommand), typeof(ICommand), typeof(SharedSearchPicker));

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public ICommand? SelectCommand
    {
        get => (ICommand?)GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    //Mientras se pone el texto del elegido no hay que volver a buscar
    private bool _eligiendo;

    public SharedSearchPicker()
    {
        InitializeComponent();

        //Al cerrarse el modal la lista flotante tiene que irse con el
        Unloaded += (_, _) => Lista.IsOpen = false;
    }

    private void CajaCambio(object sender, TextChangedEventArgs e)
    {
        if (_eligiendo)
        {
            return;
        }

        SearchCommand?.Execute(Text);
    }

    private void ResultadoElegido(object sender, SelectionChangedEventArgs e)
    {
        if (Resultados.SelectedItem is null)
        {
            return;
        }

        var elegido = Resultados.SelectedItem;

        //Se suelta la marca ANTES de avisar: al recargarse la lista el ListBox la pone en
        //nulo, y si se aviso despues el mismo elegido llegaria dos veces.
        Resultados.SelectedItem = null;
        Lista.IsOpen = false;

        _eligiendo = true;

        try
        {
            SelectCommand?.Execute(elegido);
        }
        finally
        {
            _eligiendo = false;
        }
    }

    // La lista se abre sola cuando hay resultados y se cierra cuando no queda ninguno
    private static void ItemsCambio(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SharedSearchPicker)objeto;

        if (e.OldValue is INotifyCollectionChanged anterior)
        {
            anterior.CollectionChanged -= picker.ColeccionCambio;
        }

        if (e.NewValue is INotifyCollectionChanged nueva)
        {
            nueva.CollectionChanged += picker.ColeccionCambio;
        }

        picker.Mostrar();
    }

    private void ColeccionCambio(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Mostrar();
    }

    private void Mostrar()
    {
        var hay = false;

        if (Items is not null)
        {
            foreach (var _ in Items)
            {
                hay = true;
                break;
            }
        }

        Lista.IsOpen = hay && Caja.IsKeyboardFocusWithin;
    }
}
