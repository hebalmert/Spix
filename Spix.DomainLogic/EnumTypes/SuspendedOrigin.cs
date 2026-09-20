namespace Spix.DomainLogic.EnumTypes;

//De donde salio la suspension de un contrato
public enum SuspendedOrigin
{
    //La registro una persona desde Control de Contratos o desde el modulo de suspendidos
    Manual = 1,

    //La genero el corte masivo por falta de pago (RunSuspended)
    Corte = 2
}
