using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.SupplierPage;

public partial class DetailsSupplier
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Supplier? Supplier;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }
    private bool IsLoading = true;
    private const string BaseUrl = "/api/v1/suppliers";

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var responseHttp = await _repository.GetAsync<Supplier>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            IsLoading = false;
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Supplier = responseHttp.Response;
        IsLoading = false;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
