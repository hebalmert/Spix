namespace Spix.DomainLogic.ModelUtility;

//Distancia entre dos coordenadas: compara donde dice el sistema que vive el cliente
//contra donde estuvo el tecnico. Si pasa del margen, la visita queda para revisar.
public static class GeoHelper
{
    //Margen que se acepta como "el mismo sitio", en metros. 100 es corto a proposito:
    //con un margen amplio el tecnico marca desde la entrada y se va.
    public const int MargenMetros = 100;

    //Metros entre los dos puntos (Haversine: sobre la superficie, no atravesando la
    //Tierra). Null si falta alguna coordenada: sin con que comparar no hay distancia.
    public static int? Metros(decimal? latitudA, decimal? longitudA, decimal? latitudB, decimal? longitudB)
    {
        if (latitudA is null || longitudA is null || latitudB is null || longitudB is null)
        {
            return null;
        }

        const double radioTierra = 6371000;

        var latA = Radianes((double)latitudA.Value);
        var latB = Radianes((double)latitudB.Value);
        var difLat = Radianes((double)(latitudB.Value - latitudA.Value));
        var difLon = Radianes((double)(longitudB.Value - longitudA.Value));

        var a = Math.Sin(difLat / 2) * Math.Sin(difLat / 2) +
                Math.Cos(latA) * Math.Cos(latB) *
                Math.Sin(difLon / 2) * Math.Sin(difLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return (int)Math.Round(radioTierra * c);
    }

    //Si cuentan como el mismo sitio. Sin distancia responde true: no se marca a nadie
    //por falta de datos.
    public static bool MismoSitio(int? metros)
    {
        return metros is null || metros.Value <= MargenMetros;
    }

    private static double Radianes(double grados) => grados * Math.PI / 180;
}
