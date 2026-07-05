namespace Spix.xNetwork.MapHelper;

public static class GeoDistance
{
    private const double EarthRadiusKm = 6371.0088;

    public static double Kilometers(decimal latitudeA, decimal longitudeA, decimal latitudeB, decimal longitudeB)
    {
        var lat1 = ToRadians((double)latitudeA);
        var lat2 = ToRadians((double)latitudeB);
        var deltaLat = ToRadians((double)(latitudeB - latitudeA));
        var deltaLon = ToRadians((double)(longitudeB - longitudeA));

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double value) => value * Math.PI / 180;
}
