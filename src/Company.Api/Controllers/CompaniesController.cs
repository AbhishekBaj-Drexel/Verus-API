namespace Company.Api.Controllers;

using Company.Api.DTOs.Requests;
using Company.Api.DTOs.Responses;
using Company.Application.Services;
using AppCompanyDto = Company.Application.Services.Models.CompanyDto;
using AppCompanySearchResultDto = Company.Application.Services.Models.CompanySearchResultDto;
using AppCreateCompanyRequestModel = Company.Application.Services.Models.CreateCompanyRequestModel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequestModel request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Create company request received for {CompanyName} with URL {WebsiteUrl}.",
            request.CompanyName,
            request.WebsiteUrl);

        var created = await _companyService.CreateAsync(
            new AppCreateCompanyRequestModel(
                request.CompanyName?.Trim() ?? string.Empty,
                request.WebsiteUrl?.Trim() ?? string.Empty),
            ct);

        var response = ToApiCompanyDto(created);
        using (_logger.BeginScope(new Dictionary<string, object> { ["CompanyId"] = response.Id }))
        {
            _logger.LogInformation("Company created for domain {Domain}.", response.WebsiteDomain);
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanySearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? name,
        [FromQuery] string? domain,
        [FromQuery] string? q,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Company search request received with name={Name}, domain={Domain}, q={Q}.",
            name,
            domain,
            q);

        var queryDto = new CompanySearchQuery(name, domain, q);
        var query = new Company.Application.Search.CompanySearchQuery(queryDto.Name, queryDto.Domain, queryDto.Q);

        var searchResults = await _companyService.SearchAsync(query, ct);
        _logger.LogInformation("Company search returned {Count} results.", searchResults.Count);

        if (!string.IsNullOrWhiteSpace(q) && _logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var result in searchResults.Take(5))
            {
                _logger.LogDebug(
                    "Company {CompanyId} scored {Score} with reasons: {Reasons}",
                    result.Company.Id,
                    result.RelevanceScore,
                    string.Join("; ", result.ScoreReasons));
            }
        }

        return Ok(searchResults.Select(ToApiSearchResultDto).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var company = await _companyService.GetByIdAsync(id, ct);
        if (company is null)
        {
            return NotFound();
        }

        return Ok(ToApiCompanyDto(company));
    }

    private static CompanyDto ToApiCompanyDto(AppCompanyDto company)
    {
        return new CompanyDto(
            company.Id,
            company.CompanyName,
            company.WebsiteUrl,
            company.WebsiteDomain,
            company.CreatedAt);
    }

    private static CompanySearchResultDto ToApiSearchResultDto(AppCompanySearchResultDto result)
    {
        return new CompanySearchResultDto(
            ToApiCompanyDto(result.Company),
            result.RelevanceScore,
            result.ScoreReasons);
    }

}
