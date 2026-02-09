namespace Company.Application.Abstractions;

using global::Company.Domain.Entities;

public interface ICompanyRepository
{
    Task<Company> AddAsync(Company company, CancellationToken ct);

    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct);

    Task<IReadOnlyList<Company>> QueryAsync(CompanyQuery query, CancellationToken ct);
}
