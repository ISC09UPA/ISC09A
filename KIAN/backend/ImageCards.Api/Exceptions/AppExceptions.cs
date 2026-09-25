namespace ImageCards.Api.Exceptions;

/// <summary>Base type for expected errors that map to a specific HTTP status in <c>GlobalExceptionHandler</c>.</summary>
public abstract class AppException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} '{key}' was not found.");

public sealed class RequestValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.")
{
    public RequestValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    public IDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class UnauthenticatedException()
    : AppException("The request is not authenticated.");

/// <summary>Azure Blob Storage failed or is unreachable. The message never includes URLs or SAS tokens.</summary>
public sealed class StorageUnavailableException(string message, Exception? innerException = null)
    : AppException(message, innerException);
