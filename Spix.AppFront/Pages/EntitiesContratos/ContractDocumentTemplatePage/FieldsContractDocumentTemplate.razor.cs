using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

//Editor visual de coordenadas: se elige el campo, se hace clic en el PDF y se arrastra.
//Los cambios quedan en memoria y se guardan todos juntos con Guardar.
public partial class FieldsContractDocumentTemplate : IAsyncDisposable
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractdocuments/templates";
    private const string HostId = "spixPdfFieldEditorHost";
    private static readonly ContractDocumentFieldType[] FieldTypes = Enum.GetValues<ContractDocumentFieldType>();

    private ContractDocumentTemplate? Template;
    private List<ContractDocumentTemplateField> Fields = new();
    private string? originalBase64;
    private int pageCount;
    private Guid? selectedKey;
    private ContractDocumentFieldType? placingType;
    private bool isLoading = true;
    private bool IsSaving;
    private bool isPreview;
    private bool isBusyPreview;
    private bool isDirty;

    //PDF pendiente de dibujar en el host despues del render
    private string? pendingBase64;
    private bool pendingReadOnly;
    private DotNetObjectReference<FieldsContractDocumentTemplate>? dotNetRef;
    private IJSObjectReference? editorScript;

    private ContractDocumentTemplateField? SelectedField => Fields.FirstOrDefault(x => x.ContractDocumentTemplateFieldId == selectedKey);

    private bool HasSignature => Fields.Any(x => x.FieldType == ContractDocumentFieldType.Signature);

    protected override async Task OnInitializedAsync()
    {
        //Plantilla con sus campos guardados
        var templateHttp = await _repository.GetAsync<ContractDocumentTemplate>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(templateHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Template = templateHttp.Response!;
        Fields = Template.ContractDocumentTemplateFields?.ToList() ?? new();
        foreach (var field in Fields)
            field.ContractDocumentTemplate = null;

        //PDF original para dibujarlo en pantalla
        var pdfHttp = await _repository.GetAsync<ContractDocumentPdfDTO>($"{BaseUrl}/{Id}/pdf");
        if (await _responseHandler.HandleErrorAsync(pdfHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        originalBase64 = pdfHttp.Response!.FileBase64;
        pageCount = pdfHttp.Response.PageCount;
        isLoading = false;
        QueueLoad(originalBase64, readOnly: false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingBase64 is null)
            return;

        var base64 = pendingBase64;
        var readOnly = pendingReadOnly;
        pendingBase64 = null;

        try
        {
            //El JS del editor lo carga este modal al abrirse; no esta en index.html.
            //El ?v= es obligatorio: sin el, el importmap de .NET 10 lo cambia por el nombre con huella
            //(pdfFieldEditor.xxxx.js), que no existe porque en Spix hosted la huella esta apagada (404).
            editorScript ??= await JS.InvokeAsync<IJSObjectReference>("import", "./jslib/pdfFieldEditor.js?v=20260913");

            dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeAsync<int>("spixPdfFieldEditor.load", HostId, base64, dotNetRef, readOnly);

            if (!readOnly)
                await PushFieldsAsync();
        }
        catch (JSException ex)
        {
            //Si el PDF no se puede dibujar se avisa con el motivo, sin tumbar la aplicacion
            await _sweetAlert.FireAsync("PDF", $"No fue posible mostrar el PDF: {ex.Message}", SweetAlertIcon.Error);
        }
    }

    //Acciones que llegan desde el PDF (jslib/pdfFieldEditor.js)

    [JSInvokable]
    public async Task PlaceField(int pageNumber, double positionX, double positionY)
    {
        if (placingType is null)
            return;

        var isSignature = placingType == ContractDocumentFieldType.Signature;
        var field = new ContractDocumentTemplateField
        {
            ContractDocumentTemplateFieldId = Guid.NewGuid(),
            ContractDocumentTemplateId = Id,
            FieldType = placingType.Value,
            PageNumber = pageNumber,
            PositionX = ToPoints(positionX),
            PositionY = ToPoints(positionY),
            Width = isSignature ? 180 : null,
            Height = isSignature ? 60 : null,
            FontSize = 12
        };

        Fields.Add(field);
        selectedKey = field.ContractDocumentTemplateFieldId;
        placingType = null;
        isDirty = true;

        await PushFieldsAsync();
        StateHasChanged();
    }

    [JSInvokable]
    public async Task MoveField(string key, double positionX, double positionY, double? width, double? height)
    {
        var field = FindField(key);
        if (field is null)
            return;

        field.PositionX = ToPoints(positionX);
        field.PositionY = ToPoints(positionY);
        if (width.HasValue)
            field.Width = ToPoints(width.Value);
        if (height.HasValue)
            field.Height = ToPoints(height.Value);

        selectedKey = field.ContractDocumentTemplateFieldId;
        isDirty = true;

        await PushFieldsAsync();
        StateHasChanged();
    }

    [JSInvokable]
    public async Task SelectField(string key)
    {
        selectedKey = FindField(key)?.ContractDocumentTemplateFieldId;

        await PushFieldsAsync();
        StateHasChanged();
    }

    //Acciones del panel

    private async Task StartPlacingAsync(ContractDocumentFieldType type)
    {
        placingType = placingType == type ? null : type;
        await PushFieldsAsync();
    }

    private async Task SelectFromListAsync(ContractDocumentTemplateField field)
    {
        if (!Fields.Contains(field))
            return;

        selectedKey = field.ContractDocumentTemplateFieldId;
        await PushFieldsAsync();

        if (!isPreview && editorScript is not null)
            await JS.InvokeVoidAsync("spixPdfFieldEditor.scrollToField", selectedKey.ToString());
    }

    private async Task RemoveFieldAsync(ContractDocumentTemplateField field)
    {
        Fields.Remove(field);
        if (selectedKey == field.ContractDocumentTemplateFieldId)
            selectedKey = null;

        isDirty = true;
        await PushFieldsAsync();
    }

    private async Task PageChangedAsync(ChangeEventArgs e)
    {
        if (SelectedField is null || !int.TryParse(e.Value?.ToString(), out var page))
            return;

        SelectedField.PageNumber = page;
        await FieldEditedAsync();
    }

    private async Task FieldEditedAsync()
    {
        isDirty = true;
        await PushFieldsAsync();
    }

    private void ShowEditor()
    {
        if (!isPreview || originalBase64 is null)
            return;

        isPreview = false;
        QueueLoad(originalBase64, readOnly: false);
    }

    private async Task ShowPreviewAsync()
    {
        isBusyPreview = true;
        var responseHttp = await _repository.PostAsync<List<ContractDocumentTemplateField>, ContractDocumentPdfDTO>($"{BaseUrl}/{Id}/preview", CleanFields());
        isBusyPreview = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        isPreview = true;
        placingType = null;
        QueueLoad(responseHttp.Response!.FileBase64, readOnly: true);
    }

    private async Task SaveAsync()
    {
        if (!HasSignature)
        {
            var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
            {
                Title = "Falta la firma",
                Text = "La plantilla no tiene campo de firma y el cliente no podra firmar. Desea guardar de todas formas?",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Guardar",
                CancelButtonText = "Cancelar"
            });

            if (confirm.IsDismissed || confirm.Value != "true")
                return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}/{Id}/fields", CleanFields());
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        isDirty = false;
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        if (isDirty)
        {
            var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
            {
                Title = "Cambios sin guardar",
                Text = "Si sale ahora se pierden los campos que movio o agrego. Desea salir?",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true,
                ConfirmButtonText = "Salir",
                CancelButtonText = "Seguir editando"
            });

            if (confirm.IsDismissed || confirm.Value != "true")
                return;
        }

        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    //Utilidades

    private void QueueLoad(string? base64, bool readOnly)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return;

        pendingBase64 = base64;
        pendingReadOnly = readOnly;
    }

    private async Task PushFieldsAsync()
    {
        if (isPreview || isLoading || editorScript is null)
            return;

        var items = Fields.Select(x => new EditorField(
            x.ContractDocumentTemplateFieldId.ToString(),
            (int)x.FieldType,
            GetFieldName(x.FieldType),
            x.PageNumber,
            x.PositionX,
            x.PositionY,
            x.Width,
            x.Height,
            x.FontSize));

        await JS.InvokeVoidAsync("spixPdfFieldEditor.setFields", items, selectedKey?.ToString(), (int?)placingType ?? 0);
    }

    private List<ContractDocumentTemplateField> CleanFields() =>
        Fields.Select(x => new ContractDocumentTemplateField
        {
            ContractDocumentTemplateFieldId = x.ContractDocumentTemplateFieldId,
            ContractDocumentTemplateId = Id,
            FieldType = x.FieldType,
            PageNumber = x.PageNumber,
            PositionX = x.PositionX,
            PositionY = x.PositionY,
            Width = x.Width,
            Height = x.Height,
            FontSize = x.FontSize
        }).ToList();

    private ContractDocumentTemplateField? FindField(string key) =>
        Guid.TryParse(key, out var id) ? Fields.FirstOrDefault(x => x.ContractDocumentTemplateFieldId == id) : null;

    private static decimal ToPoints(double value) => Math.Round((decimal)Math.Max(0, value), 2);

    private static string GetFieldName(ContractDocumentFieldType fieldType) =>
        fieldType switch
        {
            ContractDocumentFieldType.FullName => "Nombre",
            ContractDocumentFieldType.Document => "Documento",
            ContractDocumentFieldType.Phone => "Telefono",
            ContractDocumentFieldType.Date => "Fecha",
            ContractDocumentFieldType.Signature => "Firma",
            ContractDocumentFieldType.Address => "Direccion",
            ContractDocumentFieldType.Email => "Correo",
            ContractDocumentFieldType.PrintName => "Nombre imprenta",
            _ => fieldType.ToString()
        };

    private static string GetFieldIcon(ContractDocumentFieldType fieldType) =>
        fieldType switch
        {
            ContractDocumentFieldType.FullName => "fa fa-user",
            ContractDocumentFieldType.Document => "fa fa-id-card",
            ContractDocumentFieldType.Phone => "fa fa-phone",
            ContractDocumentFieldType.Date => "fa fa-calendar-days",
            ContractDocumentFieldType.Signature => "fa fa-signature",
            ContractDocumentFieldType.Address => "fa fa-location-dot",
            ContractDocumentFieldType.Email => "fa fa-envelope",
            ContractDocumentFieldType.PrintName => "fa fa-pen",
            _ => "fa fa-font"
        };

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (editorScript is not null)
            {
                await JS.InvokeVoidAsync("spixPdfFieldEditor.dispose");
                await editorScript.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
        }

        dotNetRef?.Dispose();
    }

    //Lo que el editor JS necesita de cada campo
    private sealed record EditorField(
        string Key,
        int FieldType,
        string Label,
        int PageNumber,
        decimal PositionX,
        decimal PositionY,
        decimal? Width,
        decimal? Height,
        int FontSize);
}
