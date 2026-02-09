namespace Company.Api.Middleware;

using Company.Api.DTOs.Responses;
using Company.Application.Services;
using Microsoft.AspNetCore.Diagnostics;

public static class ExceptionHandlingExtensions
{
    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception is CompanyValidationException validationException)
                {
                    logger.LogWarning(
                        validationException,
                        "Business validation failed for {Path}: {Errors}",
                        context.Request.Path,
                        string.Join(" | ", validationException.Errors));

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new ValidationErrorResponse(
                        Message: "Validation failed.",
                        Errors: new Dictionary<string, string[]>
                        {
                            ["company"] = validationException.Errors.ToArray(),
                        }));

                    return;
                }

                logger.LogError(exception, "Unhandled exception for {Path}.", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "An unexpected error occurred.",
                });
            });
        });

        return app;
    }
}
