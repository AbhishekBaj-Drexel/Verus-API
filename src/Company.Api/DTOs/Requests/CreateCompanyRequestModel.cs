namespace Company.Api.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public sealed class CreateCompanyRequestModel : IValidatableObject
{
    [Required]
    [MinLength(3, ErrorMessage = "Company name must contain at least a few characters.")]
    public string? CompanyName { get; init; }

    [Required]
    public string? WebsiteUrl { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(WebsiteUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            yield return new ValidationResult(
                "Website URL must be a valid, well-formed URL.",
                [nameof(WebsiteUrl)]);
        }
    }

    public CreateCompanyRequest ToRequestDto()
    {
        return new CreateCompanyRequest(
            CompanyName?.Trim() ?? string.Empty,
            WebsiteUrl?.Trim() ?? string.Empty);
    }
}
