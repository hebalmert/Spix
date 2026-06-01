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

            mac.Status = SerialStateType.Disponible;
            mac.Comment = "";

            await _context.SaveChangesAsync();

            return new ActionResponse<CargueDetail> { WasSuccess = true, Result = mac };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<CargueDetail> { WasSuccess = false, Message = ex.Message };
        }
    }


}
