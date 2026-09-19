using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

//Pantalla a la que lleva el codigo QR del documento. Pide sesion: la consulta del certificado
//va con el token del usuario.
public partial class VerifySignature
{
    [Parameter] public string? Code { get; set; }

    private string searchCode = string.Empty;
    private string? appliedCode;

    protected override void OnParametersSet()
    {
        //Si llego por el QR, el identificador viene en la direccion
        if (string.IsNullOrWhiteSpace(Code) || string.Equals(Code, appliedCode, StringComparison.OrdinalIgnoreCase))
            return;

        searchCode = Code;
        appliedCode = Code;
    }

    private void Search()
    {
        appliedCode = searchCode?.Trim();
    }
}
