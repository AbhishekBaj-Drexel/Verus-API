namespace Company.Api.DTOs.Responses;

public sealed record ValidationErrorResponse(
    string Message,
    IReadOnlyDictionary<string, string[]> Errors);
