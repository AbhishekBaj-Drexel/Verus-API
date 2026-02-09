namespace Company.Application.Search;

public sealed record CompanySearchQuery(
    string? Name,
    string? Domain,
    string? Q);
