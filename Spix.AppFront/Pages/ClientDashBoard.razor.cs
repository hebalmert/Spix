using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages;

public partial class ClientDashBoard
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    private const string BaseUrl = "api/v1/mysignatures";

    private int pendingSignatures;

    protected override async Task OnInitializedAsync()
    {
        //Solo para el contador de la tarjeta: si falla, la tarjeta igual se muestra
        var responseHttp = await _repository.GetAsync<List<MySignatureDocumentDTO>>(BaseUrl);
        if (responseHttp.Error || responseHttp.Response == null)
            return;

        pendingSignatures = responseHttp.Response.Count(x => !x.Signed);
    }

    private void GoToSignatures()
    {
        _navigationManager.NavigateTo("/my-signatures");
    }
}
