namespace Company.Api.DependencyInjection;

using Company.Application.Abstractions;
using Company.Application.Services;
using Company.Application.Validation;
using Company.Infrastructure.Persistence.InMemory;

public static class ValidationServiceCollectionExtensions
{
    // In-memory storage must survive across requests, so repository is singleton.
    // Relevance evaluator is stateless and thread-safe, so singleton is appropriate.
    // Company service is request-scoped orchestration.
    public static IServiceCollection AddCompanyValidation(this IServiceCollection services)
    {
        services.AddSingleton<ICompanyRelevanceEvaluator, TokenBasedCompanyRelevanceEvaluator>();
        services.AddSingleton<ICompanyRepository, InMemoryCompanyRepository>();
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
