namespace Company.Application.Services.Models;

public sealed record CompanyDto(
    Guid Id,
    string CompanyName,
    string WebsiteUrl,
    string WebsiteDomain,
    DateTimeOffset CreatedAt);
