namespace Company.IntegrationTests.Api;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class CompaniesApiIntegrationTests
{
    [Fact]
    public async Task Post_ValidRequest_Returns201Created()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            companyName = "Example",
            websiteUrl = "https://example.com",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CompanyDto>();
        Assert.NotNull(created);
        Assert.Equal("Example", created!.CompanyName);
    }

    [Fact]
    public async Task GetAll_AfterCreate_ContainsCreatedRecord()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCompanyAsync(client, "Example", "https://example.com");

        var allCompanies = await client.GetFromJsonAsync<List<CompanySearchResultDto>>("/api/companies");

        Assert.NotNull(allCompanies);
        Assert.Contains(allCompanies!, result => result.Company.Id == created.Id);
    }

    [Fact]
    public async Task GetById_AfterCreate_Returns200Ok()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var created = await CreateCompanyAsync(client, "Example", "https://example.com");

        var response = await client.GetAsync($"/api/companies/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<CompanyDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task GetById_WithMissingId_Returns404NotFound()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync($"/api/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidDto_Returns400WithValidationShape()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            companyName = "",
            websiteUrl = "not-a-url",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Validation failed.", payload!.Message);
        Assert.NotEmpty(payload.Errors);
    }

    [Fact]
    public async Task Post_NameNotRelevantToWebsite_Returns400WithBusinessValidationShape()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            companyName = "Blue Ocean",
            websiteUrl = "https://example.com",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Validation failed.", payload!.Message);
        Assert.True(payload.Errors.ContainsKey("company"));
        Assert.Contains(
            payload.Errors["company"],
            error => error.Contains("not relevant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Get_WithQ_ReturnsResultsOrderedByRelevanceWithMetadata()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory);

        await CreateCompanyAsync(client, "Ace Payments", "https://www.acepayments.com");
        await CreateCompanyAsync(client, "Ace Logistics", "https://www.acelogistics.com");
        await CreateCompanyAsync(client, "Global Shipping", "https://www.globalshipping.com");

        var results = await client.GetFromJsonAsync<List<CompanySearchResultDto>>(
            "/api/companies?q=ace%20payments");

        Assert.NotNull(results);
        Assert.NotEmpty(results!);

        // Global Shipping should not appear because its relevance score for the query is zero.
        Assert.DoesNotContain(results!, result => result.Company.CompanyName == "Global Shipping");

        // Highest relevance hit should be first.
        Assert.Equal("Ace Payments", results![0].Company.CompanyName);
        Assert.NotEmpty(results[0].ScoreReasons);

        for (var i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].RelevanceScore >= results[i].RelevanceScore);
        }
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>();
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    private static async Task<CompanyDto> CreateCompanyAsync(HttpClient client, string name, string websiteUrl)
    {
        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            companyName = name,
            websiteUrl,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CompanyDto>();
        Assert.NotNull(created);
        return created!;
    }

    private sealed record CompanyDto(
        Guid Id,
        string CompanyName,
        string WebsiteUrl,
        string WebsiteDomain,
        DateTimeOffset CreatedAt);

    private sealed record CompanySearchResultDto(
        CompanyDto Company,
        double RelevanceScore,
        IReadOnlyList<string> ScoreReasons);

    private sealed record ValidationErrorResponse(
        string Message,
        Dictionary<string, string[]> Errors);
}
