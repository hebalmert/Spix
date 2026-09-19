namespace Spix.AppFront.Shared;

//Apariencia del selector de foto: icono cuando no hay imagen y regla de "sin foto".
public partial class InputImageGeneral
{
    //Icono que se ve cuando el registro no tiene foto. Cada formulario puede cambiarlo:
    //<InputImageGeneral PlaceholderIcon="fa fa-truck" ... />
    [Microsoft.AspNetCore.Components.Parameter] public string PlaceholderIcon { get; set; } = "fa fa-user";

    //El backend manda la URL de "NoImage" cuando no hay foto: eso cuenta como sin foto,
    //asi se ve el icono local y no se consulta esa imagen.
    private bool HasPhoto =>
        !string.IsNullOrWhiteSpace(ImageUrl) &&
        ImageUrl!.IndexOf("NoImage", System.StringComparison.OrdinalIgnoreCase) < 0;
}
