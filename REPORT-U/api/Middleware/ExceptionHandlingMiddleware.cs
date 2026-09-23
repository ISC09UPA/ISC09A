using Microsoft.AspNetCore.Mvc;

namespace ReportU.Middleware;

/// <summary>
/// Convención global de errores: cualquier excepción no controlada se responde como
/// `application/problem+json` (RFC 7807) con extensiones `code` y `traceId`.
/// Los 400 de validación, 401/403 de autenticación y los Problem() de controladores
/// ya salen en ese formato de forma nativa.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (FileNotFoundException ex)
        {
            logger.LogWarning(ex, "Recurso de almacenamiento no encontrado.");
            await WriteProblemAsync(context, 404, "blob_not_found", "Archivo no encontrado en el almacenamiento.");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Solicitud inválida.");
            await WriteProblemAsync(context, 400, "bad_request", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado.");
            await WriteProblemAsync(context, 500, "internal_error", "Ocurrió un error inesperado.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int status, string code, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                400 => "Solicitud inválida",
                404 => "No encontrado",
                _ => "Error interno",
            },
            Detail = detail,
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }
}

/// <summary>Códigos de error propios (extensión `code` del ProblemDetails) para que la app móvil los maneje sin parsear mensajes.</summary>
public static class ErrorCodes
{
    public const string ValidationError = "validation_error";
    public const string Unauthorized = "unauthorized";
    public const string ForbiddenNotOwner = "forbidden_not_owner";
    public const string NotFound = "not_found";
    public const string EmailTaken = "email_taken";
    public const string UsernameTaken = "username_taken";
    public const string InvalidCredentials = "invalid_credentials";
    public const string AlreadySupported = "already_supported";
    public const string NotSupported = "not_supported";
    public const string AlreadyBookmarked = "already_bookmarked";
    public const string NotBookmarked = "not_bookmarked";
    public const string ImageLimitReached = "image_limit_reached";
    public const string UnsupportedMediaType = "unsupported_media_type";
    public const string PayloadTooLarge = "payload_too_large";
    public const string BlobNotFound = "blob_not_found";
    public const string BlobError = "blob_error";
}
