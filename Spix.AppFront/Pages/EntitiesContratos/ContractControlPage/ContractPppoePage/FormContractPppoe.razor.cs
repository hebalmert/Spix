using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.AppFront.Helper;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Text;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPppoePage;

public partial class FormContractPppoe
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public ContractPppoe ContractPppoe { get; set; } = null!;

    //Editando se puede cambiar el estado del acceso; creando no, porque nace Activa
    [Parameter] public bool IsEdit { get; set; }

    //Los estados que se pueden elegir. Llegan ARMADOS del backend, con su neutro y
    //traducidos: aqui no se filtra, no se ordena y no se agrega ninguna opcion.
    private List<IntItemModel>? AccessStates;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    //El numero del contrato y el apellido del cliente: con eso se arma el usuario que
    //propone el sistema. El numero es unico por corporacion y el apellido dice de quien es.
    [Parameter] public string? ControlContrato { get; set; }

    [Parameter] public string? ClientLastName { get; set; }

    //Sin las que se confunden al dictar por telefono: 0 y O, 1 y l
    private const string Alfabeto = "23456789abcdefghjkmnpqrstuvwxyz";

    protected override async Task OnInitializedAsync()
    {
        //La lista solo hace falta cuando hay combo que pintar
        if (!IsEdit || ContractPppoe.PppoeAccessState == PppoeAccessState.Corte)
        {
            return;
        }

        var responseHttp = await _repository.GetAsync<List<IntItemModel>>("/api/v1/contractpppoes/accessStates");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        AccessStates = responseHttp.Response;
    }

    private void AccessStateChanged(ChangeEventArgs e)
    {
        ContractPppoe.PppoeAccessState = (PppoeAccessState)Convert.ToInt32(e.Value);
    }

    //Solo llega aqui si el modelo paso la validacion
    private async Task HandleValidSubmitAsync()
    {
        await OnSubmit.InvokeAsync();
    }

    //Devuelve el usuario a lo que el sistema propone. Sirve cuando el operador lo cambio
    //y quiere volver a la convencion de la casa.
    private void ProponerUsuario()
    {
        ContractPppoe.Usuario = ProponerUsuario(ClientLastName, ControlContrato);
    }

    //El usuario PPPoE que propone el sistema: apellido + numero de contrato + 3 al azar.
    //Ej: contrato 1 de Merchan -> "mer1k7q". Se reconoce de quien es, lleva el numero del
    //contrato y no se adivina. Lo usan el boton # y el modal de crear, para que lo que
    //aparece de entrada ya cumpla la regla de la entidad.
    public static string ProponerUsuario(string? apellido, string? controlContrato)
    {
        var prefijo = SoloLetras(apellido);
        var numero = SoloNumeros(controlContrato);

        //Sin apellido usable queda solo numero + azar; el prefijo fijo mantiene la letra
        //que exige la regla y el largo minimo.
        if (prefijo.Length == 0)
        {
            prefijo = "cli";
        }

        if (numero.Length == 0)
        {
            numero = "0";
        }

        return prefijo + numero + AlAzar(3);
    }

    //Las primeras 3 letras del apellido, en minuscula y sin tildes ni ñ: un usuario PPPoE
    //viaja por RADIUS y por el equipo, no es sitio para caracteres raros.
    private static string SoloLetras(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var limpio = new StringBuilder();
        foreach (var letra in texto.Normalize(NormalizationForm.FormD))
        {
            var minuscula = char.ToLowerInvariant(letra);
            if (minuscula >= 'a' && minuscula <= 'z')
            {
                limpio.Append(minuscula);
            }

            if (limpio.Length == 3)
            {
                break;
            }
        }

        return limpio.ToString();
    }

    private static string SoloNumeros(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var limpio = new StringBuilder();
        foreach (var caracter in texto)
        {
            if (char.IsDigit(caracter))
            {
                limpio.Append(caracter);
            }
        }

        return limpio.ToString();
    }

    private static string AlAzar(int largo)
    {
        var texto = new StringBuilder(largo);
        for (var i = 0; i < largo; i++)
        {
            texto.Append(Alfabeto[Random.Shared.Next(Alfabeto.Length)]);
        }

        return texto.ToString();
    }

    //Una clave corta y al azar, como hace cualquier sistema de ISP para no dejarla en blanco
    private void GenerarClave()
    {
        ContractPppoe.Clave = Guid.NewGuid().ToString("N").Substring(0, 10);
    }
}
