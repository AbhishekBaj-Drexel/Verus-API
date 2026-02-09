namespace Company.Api.DTOs.Responses;

public sealed record CompanyDto(
    Guid Id,
    string CompanyName,
    string WebsiteUrl,
    string WebsiteDomain,
    DateTimeOffset CreatedAt);
