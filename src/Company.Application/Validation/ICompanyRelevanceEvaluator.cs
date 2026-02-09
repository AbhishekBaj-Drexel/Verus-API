namespace Company.Application.Validation;

public interface ICompanyRelevanceEvaluator
{
    Task<RelevanceEvaluation> EvaluateAsync(string companyName, Uri websiteUrl, CancellationToken ct);
}
