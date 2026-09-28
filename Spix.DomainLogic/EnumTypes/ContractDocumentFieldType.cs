namespace Spix.DomainLogic.EnumTypes;

//Lo que se puede colocar sobre una plantilla PDF.
//
//La regla para agregar uno: el dato tiene que existir CUANDO SE FIRMA el contrato, y
//Spix lo tiene que saber solo.
//
//Por eso NO hay campos de equipo, MAC, serial ni coordenadas: todo eso se arma en
//DetailContractControl al INSTALAR, dias despues de firmar, asi que el campo saldria
//vacio casi siempre.
//
//Y tampoco hay un campo de texto libre: el documento se firma en pantalla, no se
//imprime para llenarlo a mano, asi que un recuadro vacio no lo completa nadie.
public enum ContractDocumentFieldType
{
    FullName = 1,
    Document = 2,
    Phone = 3,
    Date = 4,
    Signature = 5,
    Address = 6,
    Email = 7,
    PrintName = 8,

    //===== Del contrato =====

    ContractNumber = 9,

    //Fecha Y hora. El campo Date de arriba solo pone la fecha.
    DateTime = 10,

    //===== Del plan: existen desde que se crea el contrato =====

    PlanName = 11,
    SpeedDown = 12,
    SpeedUp = 13,
    MonthlyPrice = 14
}
