namespace Company.Api.DTOs.Requests;

public sealed record CreateCompanyRequest(
    string CompanyName,
    string WebsiteUrl);
