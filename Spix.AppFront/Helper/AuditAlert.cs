using CurrieTechnologies.Razor.SweetAlert2;

namespace Spix.AppFront.Helper;

//La ventana de auditoria de cualquier modulo: etiqueta en gris y el dato debajo.
//Cada pantalla pasa los datos que tenga; las lineas sin valor no se pintan.
public static class AuditAlert
{
    public static async Task ShowAsync(
        SweetAlertService sweetAlert,
        string titulo,
        params (string Etiqueta, string? Valor)[] datos)
    {
        //Un registro viejo puede no tener nada guardado: la ventana no se abre vacia
        if (datos.All(x => string.IsNullOrWhiteSpace(x.Valor)))
        {
            datos = new[] { (titulo, (string?)"-") };
        }

        var lineas = datos
            .Where(x => !string.IsNullOrWhiteSpace(x.Valor))
            .Select(x =>
                "<div style=\"margin-bottom:8px\">" +
                $"<div style=\"font-size:.8rem;color:#6c7a91\">{x.Etiqueta}</div>" +
                $"<div style=\"font-weight:600\">{x.Valor}</div></div>");

        await sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = titulo,
            Html = $"<div style=\"text-align:left\">{string.Concat(lineas)}</div>",
            Icon = SweetAlertIcon.Info
        });
    }
}
