namespace Company.Infrastructure.Persistence.InMemory;

using System.Collections.Concurrent;
using Company.Application.Abstractions;
using global::Company.Domain.Entities;
using global::Company.Domain.ValueObjects;

public sealed class InMemoryCompanyRepository : ICompanyRepository
{
    private readonly ConcurrentDictionary<Guid, Company> _storage = new();

    public Task<Company> AddAsync(Company company, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(company);

        var stored = Clone(company);
        if (!_storage.TryAdd(stored.Id, stored))
        {
            throw new InvalidOperationException($"A company with id '{stored.Id}' already exists.");
        }

        return Task.FromResult(Clone(stored));
    }

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_storage.TryGetValue(id, out var company) ? Clone(company) : null);
    }

    public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult<IReadOnlyList<Company>>(
            _storage.Values
                .OrderByDescending(company => company.CreatedAt)
                .Select(Clone)
                .ToArray());
    }

    public Task<IReadOnlyList<Company>> QueryAsync(CompanyQuery query, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(query);

        var normalizedNameContains = query.NameContains?.Trim();
        var normalizedDomainEquals = NormalizeDomain(query.DomainEquals);

        var results = _storage.Values
            .Where(company =>
                string.IsNullOrWhiteSpace(normalizedNameContains) ||
                company.Name.Contains(normalizedNameContains, StringComparison.OrdinalIgnoreCase))
            .Where(company =>
                string.IsNullOrWhiteSpace(normalizedDomainEquals) ||
                string.Equals(company.WebsiteDomain, normalizedDomainEquals, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(company => company.CreatedAt)
            .Select(Clone)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Company>>(results);
    }

    private static Company Clone(Company source)
    {
        return Company.Create(source.Id, source.Name, source.WebsiteUrl, source.CreatedAt);
    }

    private static string? NormalizeDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        var candidate = domain.Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return DomainParser.GetRegistrableDomain(uri);
        }

        candidate = candidate.ToLowerInvariant();
        return candidate.StartsWith("www.", StringComparison.Ordinal)
            ? candidate[4..]
            : candidate;
    }
}
