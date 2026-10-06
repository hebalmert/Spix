using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Spix.AppInfra;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using System.ComponentModel.DataAnnotations;

namespace Spix.xNetwork.MacHelper;

public class MacControl : IMacControl
{
    private readonly DataContext _context;

    public MacControl(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<CargueDetail>> SelectMacWhenAdd(Guid id, string description, IDbContextTransaction transaction)
    {
        try
        {
            var mac = await _context.CargueDetails.FirstOrDefaultAsync(x => x.CargueDetailId == id && x.Status == DomainLogic.EnumTypes.SerialStateType.Disponible);
            if (mac == null)
            {
                await transaction.RollbackAsync();
                return new ActionResponse<CargueDetail> { WasSuccess = false, Message = "MAC no Encontrada o Averiada" };
            }
            mac.Comment = description;
            mac.Status = SerialStateType.Operativo;

            //El equipo queda instalado: deja de estar disponible en la bodega.
            //Sin esto el inventario mostraba siempre el total comprado y nunca bajaba.
            await MoverStockAsync(mac.CargueDetailId, -1);

            await _context.SaveChangesAsync();

            return new ActionResponse<CargueDetail> { WasSuccess = true, Result = mac };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<CargueDetail> { WasSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ActionResponse<CargueDetail>> SelectMacToDelete(Guid id, IDbContextTransaction transaction)
    {
        try
        {
            var mac = await _context.CargueDetails.FirstOrDefaultAsync(c => c.CargueDetailId == id);
            if (mac == null)
                return new ActionResponse<CargueDetail> { WasSuccess = false, Message = "MAC no encontrada" };

            //Solo devuelve al inventario lo que de verdad estaba instalado: si ya estaba
            //Disponible, sumar otra vez inflaria el stock.
            var estabaInstalado = mac.Status == SerialStateType.Operativo;

            mac.Status = SerialStateType.Disponible;
            mac.Comment = "";

            if (estabaInstalado)
            {
                await MoverStockAsync(mac.CargueDetailId, 1);
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<CargueDetail> { WasSuccess = true, Result = mac };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<CargueDetail> { WasSuccess = false, Message = ex.Message };
        }
    }

    //Mueve la existencia del producto en SU bodega cuando un serial entra o sale de uso.
    //
    //La bodega del serial es la de la compra que lo trajo: CargueDetail -> Cargue ->
    //PurchaseDetail -> Purchase. No se guarda en el serial, se deduce de ahi.
    private async Task MoverStockAsync(Guid cargueDetailId, decimal cantidad)
    {
        var datos = await _context.CargueDetails.AsNoTracking()
            .Where(x => x.CargueDetailId == cargueDetailId)
            .Select(x => new
            {
                x.Cargue!.ProductId,
                x.Cargue.PurchaseDetail!.Purchase!.ProductStorageId
            })
            .FirstOrDefaultAsync();

        if (datos == null)
        {
            return;
        }

        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(x => x.ProductId == datos.ProductId &&
                                      x.ProductStorageId == datos.ProductStorageId);

        if (stock == null)
        {
            return;
        }

        stock.Stock += cantidad;
    }
}
