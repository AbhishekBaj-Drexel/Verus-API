namespace Company.Domain.Entities;

using global::Company.Domain.ValueObjects;

public sealed class Company
{
    public Guid Id { get; }

    public string Name { get; private set; }

    public Uri WebsiteUrl { get; private set; }

    public string WebsiteDomain { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    private Company(Guid id, string name, Uri websiteUrl, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        WebsiteUrl = websiteUrl;
        WebsiteDomain = DomainParser.GetRegistrableDomain(websiteUrl);
        CreatedAt = createdAt;
    }

    public static Company Create(Guid id, string name, Uri websiteUrl, DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Company name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(websiteUrl);

        return new Company(id, name.Trim(), websiteUrl, createdAt ?? DateTimeOffset.UtcNow);
    }

    public void UpdateWebsite(Uri websiteUrl)
    {
        ArgumentNullException.ThrowIfNull(websiteUrl);

        WebsiteUrl = websiteUrl;
        WebsiteDomain = DomainParser.GetRegistrableDomain(websiteUrl);
    }
}
