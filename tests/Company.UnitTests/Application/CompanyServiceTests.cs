namespace Company.UnitTests.Application;

using Company.Application.Search;
using Company.Application.Services;
using Company.Application.Services.Models;
using Company.Application.Validation;
using Company.Infrastructure.Persistence.InMemory;
using global::Company.Domain.Entities;

public sealed class CompanyServiceTests
{
    private static CompanyService CreateSut(InMemoryCompanyRepository repository)
    {
        return new CompanyService(repository, new TokenBasedCompanyRelevanceEvaluator());
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsNotRelevantToWebsite_ThrowsBusinessValidationException()
    {
        var repository = new InMemoryCompanyRepository();
        var sut = CreateSut(repository);

        var request = new CreateCompanyRequestModel(
            CompanyName: "Blue Ocean",
            WebsiteUrl: "https://example.com");

        var exception = await Assert.ThrowsAsync<CompanyValidationException>(() =>
            sut.CreateAsync(request, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.Contains("not relevant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_StoresCompanyAndReturnsDto()
    {
        var repository = new InMemoryCompanyRepository();
        var sut = CreateSut(repository);

        var request = new CreateCompanyRequestModel(
            CompanyName: "Example",
            WebsiteUrl: "https://example.com");

        var created = await sut.CreateAsync(request, CancellationToken.None);
        var persisted = await repository.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Example", created.CompanyName);
        Assert.Equal("example.com", created.WebsiteDomain);
        Assert.NotNull(persisted);
        Assert.Equal(created.Id, persisted!.Id);
    }

    [Fact]
    public async Task SearchAsync_WithQ_OrdersByScoreDescendingThenCreatedAtDescending()
    {
        var repository = new InMemoryCompanyRepository();

        var older = Company.Create(
            Guid.NewGuid(),
            "Acme Labs",
            new Uri("https://labs.example.com"),
            createdAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var newer = Company.Create(
            Guid.NewGuid(),
            "Acme Systems",
            new Uri("https://systems.example.com"),
            createdAt: new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));

        var perfectMatch = Company.Create(
            Guid.NewGuid(),
            "Acme",
            new Uri("https://acme.com"),
            createdAt: new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero));

        await repository.AddAsync(older, CancellationToken.None);
        await repository.AddAsync(newer, CancellationToken.None);
        await repository.AddAsync(perfectMatch, CancellationToken.None);

        var sut = CreateSut(repository);

        var results = await sut.SearchAsync(
            new CompanySearchQuery(
                Name: null,
                Domain: null,
                Q: "acme"),
            CancellationToken.None);

        Assert.Equal(3, results.Count);
        Assert.Equal(perfectMatch.Id, results[0].Company.Id);

        // older/newer both score 0.6 from name token match, so tie-break is CreatedAt desc.
        Assert.Equal(newer.Id, results[1].Company.Id);
        Assert.Equal(older.Id, results[2].Company.Id);
    }
}
