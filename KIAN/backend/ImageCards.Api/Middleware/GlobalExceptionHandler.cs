using ImageCards.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImageCards.Api.Middleware;

/// <summary>
/// Converts every unhandled exception into an RFC 9457 problem+json response.
/// Stack traces and internal messages are only exposed in the Development environment.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // Non-standard but widely used status for "client closed request".
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request {Method} {Path} was cancelled by the client",
                httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var problem = CreateProblem(exception);
        Log(httpContext, exception, problem.Status!.Value);

        httpContext.Response.StatusCode = problem.Status.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    private ProblemDetails CreateProblem(Exception exception) => exception switch
    {
        RequestValidationException validation => new ValidationProblemDetails(validation.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = validation.Message,
        },
        NotFoundException notFound => Problem(StatusCodes.Status404NotFound, "Resource not found", notFound.Message),
        UnauthenticatedException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", null),
        StorageUnavailableException storage =>
            Problem(StatusCodes.Status503ServiceUnavailable, "Image storage unavailable", storage.Message),
        BadHttpRequestException badRequest =>
            Problem(badRequest.StatusCode, "Bad request", environment.IsDevelopment() ? badRequest.Message : null),
        _ => Problem(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred",
            environment.IsDevelopment() ? exception.ToString() : null),
    };

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private void Log(HttpContext httpContext, Exception exception, int status)
    {
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Request {Method} {Path} failed with {StatusCode}",
                httpContext.Request.Method, httpContext.Request.Path, status);
        }
        else
        {
            logger.LogInformation("Request {Method} {Path} returned {StatusCode}: {ErrorType}",
                httpContext.Request.Method, httpContext.Request.Path, status, exception.GetType().Name);
        }
    }
}
