namespace Company.Application.Services;

using Company.Application.Search;
using Company.Application.Services.Models;

public interface ICompanyService
{
    Task<CompanyDto> CreateAsync(CreateCompanyRequestModel request, CancellationToken ct);

    Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken ct);

    Task<IReadOnlyList<CompanySearchResultDto>> SearchAsync(CompanySearchQuery query, CancellationToken ct);
}
