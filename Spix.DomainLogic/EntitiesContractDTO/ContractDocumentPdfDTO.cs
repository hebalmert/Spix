namespace Spix.DomainLogic.EntitiesContractDTO;

//PDF que se entrega al editor visual de coordenadas (original o vista previa con datos de prueba).
//Va en base64 y no se guarda en Azure.
public class ContractDocumentPdfDTO
{
    public string? FileBase64 { get; set; }

    public int PageCount { get; set; }
}
