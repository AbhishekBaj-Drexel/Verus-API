namespace Company.Domain.ValueObjects;

public static class DomainParser
{
    public static string GetRegistrableDomain(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (!url.IsAbsoluteUri)
        {
            throw new ArgumentException("Website URL must be an absolute URI.", nameof(url));
        }

        var host = url.Host.Trim().ToLowerInvariant();

        return host.StartsWith("www.", StringComparison.Ordinal)
            ? host[4..]
            : host;
    }
}
