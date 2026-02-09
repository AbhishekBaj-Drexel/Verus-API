using Company.Api.DependencyInjection;
using Company.Api.DTOs.Responses;
using Company.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ModelValidation");

        var errorCount = errors.Sum(entry => entry.Value.Length);
        logger.LogWarning(
            "Request validation failed for {Path} with {ErrorCount} errors.",
            context.HttpContext.Request.Path,
            errorCount);

        return new BadRequestObjectResult(new ValidationErrorResponse(
            Message: "Validation failed.",
            Errors: errors));
    };
});
builder.Services.AddCompanyValidation();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();
app.UseCors("ui");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
