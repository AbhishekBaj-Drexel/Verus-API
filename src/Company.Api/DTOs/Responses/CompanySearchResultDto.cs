namespace Company.Api.DTOs.Responses;

public sealed record CompanySearchResultDto(
    CompanyDto Company,
    double RelevanceScore,
    IReadOnlyList<string> ScoreReasons);
