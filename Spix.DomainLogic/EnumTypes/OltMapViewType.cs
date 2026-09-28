namespace Spix.DomainLogic.EnumTypes;

//Como se pinta el Mapa de OLT
public enum OltMapViewType
{
    //La OLT y sus clientes, sin lineas
    Points = 1,

    //Una linea de la OLT a cada cliente con ubicacion
    Lines = 2,

    //Las lineas con la distancia de cada cliente encima
    LinesWithDistance = 3
}
