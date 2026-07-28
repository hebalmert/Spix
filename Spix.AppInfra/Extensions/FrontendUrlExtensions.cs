namespace Spix.AppInfra.Extensions;

public static class FrontendUrlExtensions
{
    public static string CombineFrontendUrl(this string frontendUrl, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            throw new ArgumentException("The frontend URL is required.", nameof(frontendUrl));
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("The relative path is required.", nameof(relativePath));
        }

        return $"{frontendUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}
