namespace Spix.AppWpf.SharedComponents;

// Permite que el ModalService entregue parametros al contenido antes de mostrarlo.
public interface ISharedModalContent
{
    void SetParameters(IReadOnlyDictionary<string, object>? parameters);
}
