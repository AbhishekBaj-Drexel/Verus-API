namespace Company.Application.Services;

using Company.Application.Abstractions;
using Company.Application.Search;
using Company.Application.Services.Models;
using Company.Application.Validation;
using global::Company.Domain.Entities;
using global::Company.Domain.ValueObjects;

public sealed class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;
    private readonly ICompanyRelevanceEvaluator _relevanceEvaluator;

    public CompanyService(ICompanyRepository repository, ICompanyRelevanceEvaluator relevanceEvaluator)
    {
        _repository = repository;
        _relevanceEvaluator = relevanceEvaluator;
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequestModel request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var companyName = request.CompanyName?.Trim() ?? string.Empty;
        var websiteUrlRaw = request.WebsiteUrl?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new CompanyValidationException(["Company name is required."]);
        }

        if (companyName.Length < 3)
        {
            throw new CompanyValidationException(["Company name must contain at least a few characters."]);
        }

        if (!TryParseWebsiteUrl(websiteUrlRaw, out var websiteUrl))
        {
            throw new CompanyValidationException(["Website URL must be a valid, well-formed URL."]);
        }

        var relevance = await _relevanceEvaluator.EvaluateAsync(companyName, websiteUrl!, ct);
        if (!relevance.IsRelevant)
        {
            var reasons = new List<string> { "Company name is not relevant to website URL." };
            reasons.AddRange(relevance.Reasons);
            throw new CompanyValidationException(reasons);
        }

        var company = Company.Create(
            id: Guid.NewGuid(),
            name: companyName,
            websiteUrl: websiteUrl!);

        var stored = await _repository.AddAsync(company, ct);
        return ToDto(stored);
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var company = await _repository.GetByIdAsync(id, ct);
        return company is null ? null : ToDto(company);
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken ct)
    {
        var companies = await _repository.GetAllAsync(ct);
        return companies.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<CompanySearchResultDto>> SearchAsync(CompanySearchQuery query, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(query);

        var candidates = await _repository.QueryAsync(
            new CompanyQuery(query.Name, query.Domain),
            ct);

        if (candidates.Count == 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(query.Q))
        {
            return candidates
                .Select(company => new CompanySearchResultDto(
                    Company: ToDto(company),
                    RelevanceScore: 0,
                    ScoreReasons: ["No free-text query supplied; returning filtered results."]))
                .ToArray();
        }

        var qTokens = NameNormalizer.Normalize(query.Q);
        if (qTokens.Count == 0)
        {
            return [];
        }

        var scored = new List<CompanySearchResultDto>(candidates.Count);
        foreach (var company in candidates)
        {
            var (score, reasons) = CalculateSearchScore(company, qTokens);
            if (score <= 0)
            {
                continue;
            }

            scored.Add(new CompanySearchResultDto(
                Company: ToDto(company),
                RelevanceScore: score,
                ScoreReasons: reasons));
        }

        return scored
            .OrderByDescending(result => result.RelevanceScore)
            .ThenByDescending(result => result.Company.CreatedAt)
            .ToArray();
    }

    private static (double Score, IReadOnlyList<string> Reasons) CalculateSearchScore(
        Company company,
        IReadOnlyList<string> qTokens)
    {
        var reasons = new List<string>();
        var score = 0d;

        var companyNameTokens = NameNormalizer.Normalize(company.Name).ToHashSet(StringComparer.Ordinal);
        var domainTokens = TokenizeDomain(company.WebsiteDomain).ToHashSet(StringComparer.Ordinal);

        foreach (var token in qTokens)
        {
            if (companyNameTokens.Contains(token))
            {
                score += 0.6;
                reasons.Add($"Name token match: {token}");
            }

            if (domainTokens.Contains(token))
            {
                score += 0.4;
                reasons.Add($"Domain token match: {token}");
            }
        }

        return (Math.Min(1d, score), reasons);
    }

    private static IEnumerable<string> TokenizeDomain(string domain)
    {
        return domain
            .Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(NameNormalizer.Normalize)
            .Distinct(StringComparer.Ordinal);
    }

    private static bool TryParseWebsiteUrl(string rawUrl, out Uri? websiteUrl)
    {
        var parsed = Uri.TryCreate(rawUrl, UriKind.Absolute, out websiteUrl) &&
                     (websiteUrl!.Scheme == Uri.UriSchemeHttp || websiteUrl.Scheme == Uri.UriSchemeHttps);

        return parsed;
    }

    private static CompanyDto ToDto(Company company)
    {
        return new CompanyDto(
            company.Id,
            company.Name,
            company.WebsiteUrl.AbsoluteUri,
            company.WebsiteDomain,
            company.CreatedAt);
    }
}
