using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioApp.Web.Errors;

/// <summary>
/// Turns unhandled exceptions into ProblemDetails. KeyNotFoundException (404), ArgumentException (400)
/// and InvalidOperationException (409) are treated as rule violations and keep their message. That
/// includes such exceptions thrown by libraries, so their messages reach the client too (see README).
/// Anything else is logged and answered with a generic 500.
/// </summary>
internal sealed class ProblemExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ProblemExceptionHandler> _log;

    public ProblemExceptionHandler(IProblemDetailsService problemDetails, ILogger<ProblemExceptionHandler> log)
    {
        _problemDetails = problemDetails;
        _log = log;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            BadHttpRequestException bad => (bad.StatusCode, "Invalid request", "The request body or parameters could not be read."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation rejected", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred."),
        };

        if (status >= StatusCodes.Status500InternalServerError)
            _log.LogError(exception, "Unhandled exception for {Method} {Path}", http.Request.Method, http.Request.Path);
        else
            _log.LogInformation("{Method} {Path} rejected with {Status}: {Message}", http.Request.Method, http.Request.Path, status, exception.Message);

        http.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
