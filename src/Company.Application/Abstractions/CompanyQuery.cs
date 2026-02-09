namespace Company.Application.Abstractions;

public sealed record CompanyQuery(string? NameContains, string? DomainEquals);
