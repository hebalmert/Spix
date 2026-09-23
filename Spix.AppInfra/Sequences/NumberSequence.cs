using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppInfra.Sequences;

//El consecutivo lo entrega la base, no la memoria.
//
//Antes se leia la fila de Registers, se incrementaba en el objeto y se guardaba al final:
//dos usuarios podian leer el mismo valor y los dos escribian el siguiente. El segundo
//chocaba contra el indice unico de la factura y perdia toda su operacion.
//
//Con un UPDATE que devuelve lo que acaba de escribir, la base bloquea esa fila mientras
//dura la transaccion: el segundo espera su turno y se lleva el numero que sigue.
public static class NumberSequence
{
    public static async Task<int> NextAsync(DataContext context, int corporationId, NumberKind kind)
    {
        var columna = ColumnName(kind);
        var sql = $"UPDATE Registers SET [{columna}] = [{columna}] + 1 " +
                  $"OUTPUT INSERTED.[{columna}] AS [Value] WHERE CorporationId = {{0}}";

        var numeros = await context.Database.SqlQueryRaw<int>(sql, corporationId).ToListAsync();
        if (numeros.Count > 0)
            return numeros[0];

        //La corporacion todavia no tiene su fila de consecutivos: se crea y se vuelve a pedir
        context.Registers.Add(new Register
        {
            RegisterId = Guid.NewGuid(),
            CorporationId = corporationId
        });

        await context.SaveChangesAsync();

        numeros = await context.Database.SqlQueryRaw<int>(sql, corporationId).ToListAsync();
        return numeros.Count > 0 ? numeros[0] : 1;
    }

    //Solo columnas de esta lista: el nombre nunca llega desde afuera
    private static string ColumnName(NumberKind kind) => kind switch
    {
        NumberKind.Invoice => "Factura",
        NumberKind.CollectionNote => "NotaCobro",
        NumberKind.Purchase => "RegPurchase",
        NumberKind.Cargue => "Cargue",
        NumberKind.Transfer => "RegTransfer",
        NumberKind.ContractorPayment => "PagoContratista",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
