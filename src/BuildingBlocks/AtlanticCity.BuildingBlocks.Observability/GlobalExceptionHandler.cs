using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.BuildingBlocks.Observability;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (
            exception is OperationCanceledException &&
            httpContext.RequestAborted
                .IsCancellationRequested)
        {
            return false;
        }

        var error =
            MapException(
                exception);

        logger.LogError(
            exception,
            "Unhandled exception. ErrorCode {ErrorCode}.",
            error.Code);

        var problemDetails =
            new ProblemDetails
            {
                Status =
                    error.StatusCode,

                Title =
                    error.Title,

                Detail =
                    environment.IsDevelopment()
                        ? exception.Message
                        : error.Detail,

                Instance =
                    httpContext.Request.Path
            };

        problemDetails.Extensions[
            "code"] =
            error.Code;

        problemDetails.Extensions[
            "correlationId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            error.StatusCode;

        return await problemDetailsService
            .TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext =
                        httpContext,

                    ProblemDetails =
                        problemDetails
                });
    }

    private static ExceptionMapping
        MapException(
            Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException =>
                new ExceptionMapping(
                    StatusCodes
                        .Status400BadRequest,
                    "Solicitud inválida",
                    "BAD_REQUEST",
                    "La solicitud enviada no es válida."),

            ArgumentException =>
                new ExceptionMapping(
                    StatusCodes
                        .Status400BadRequest,
                    "Solicitud inválida",
                    "VALIDATION_ERROR",
                    "Uno o más datos de la solicitud no son válidos."),

            KeyNotFoundException =>
                new ExceptionMapping(
                    StatusCodes
                        .Status404NotFound,
                    "Recurso no encontrado",
                    "RESOURCE_NOT_FOUND",
                    "El recurso solicitado no existe."),

            UnauthorizedAccessException =>
                new ExceptionMapping(
                    StatusCodes
                        .Status403Forbidden,
                    "Acceso denegado",
                    "FORBIDDEN",
                    "No cuenta con permisos para realizar esta operación."),

            _ =>
                new ExceptionMapping(
                    StatusCodes
                        .Status500InternalServerError,
                    "Error interno del servidor",
                    "UNEXPECTED_ERROR",
                    "Ocurrió un error inesperado al procesar la solicitud.")
        };
    }

    private sealed record ExceptionMapping(
        int StatusCode,
        string Title,
        string Code,
        string Detail);
}