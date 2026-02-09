namespace Company.Application.Services;

public sealed class CompanyValidationException : Exception
{
    public CompanyValidationException(IReadOnlyList<string> errors)
        : base("Company validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
