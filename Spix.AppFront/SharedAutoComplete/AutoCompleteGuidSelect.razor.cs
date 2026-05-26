using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Spix.AppFront.Helper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.SharedAutoComplete;

public partial class AutoCompleteGuidSelect
{
    [Inject] private IRepository Repository { get; set; } = null!;
    [Inject] private HttpResponseHandler ResponseHandler { get; set; } = null!;

    [Parameter] public string Label { get; set; } = "Buscar";
    [Parameter] public string ApiUrl { get; set; } = null!;
    [Parameter] public int MinLength { get; set; } = 3;
    [Parameter] public EventCallback<GuidItemModel> OnSelected { get; set; }

    private string SearchText = "";
    private int SelectedIndex = -1;
    private List<GuidItemModel>? Results;
    private CancellationTokenSource? cts;

    private async Task OnSearchChanged()
    {
        if (SearchText.Length < MinLength)
        {
            Results = null;
            return;
        }

        cts?.Cancel();
        cts = new CancellationTokenSource();

        try
        {
            var response = await Repository.GetAsync<List<GuidItemModel>>(
                $"{ApiUrl}?filter={SearchText}",
                cts.Token
            );

            if (!await ResponseHandler.HandleErrorAsync(response))
            {
                Results = response.Response;
            }
        }
        catch
        {
            // Ignorar cancelaciones
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Results == null || Results.Count == 0)
            return;

        if (e.Key == "ArrowDown")
        {
            SelectedIndex = (SelectedIndex + 1) % Results.Count;
        }
        else if (e.Key == "ArrowUp")
        {
            SelectedIndex = (SelectedIndex - 1 + Results.Count) % Results.Count;
        }
        else if (e.Key == "Enter")
        {
            if (SelectedIndex >= 0)
            {
                await SelectItem(Results[SelectedIndex]);
            }
        }
    }

    private async Task SelectItem(GuidItemModel item)
    {
        SearchText = item.Name!;
        Results = null;
        await OnSelected.InvokeAsync(item);
    }
}