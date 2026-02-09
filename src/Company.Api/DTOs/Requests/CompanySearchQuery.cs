namespace Company.Api.DTOs.Requests;

public sealed record CompanySearchQuery(
    string? Name,
    string? Domain,
    string? Q);
