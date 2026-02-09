namespace Company.Application.Services.Models;

public sealed record CompanySearchResultDto(
    CompanyDto Company,
    double RelevanceScore,
    IReadOnlyList<string> ScoreReasons);
