namespace Company.Application.Services.Models;

public sealed record CreateCompanyRequestModel(
    string CompanyName,
    string WebsiteUrl);
