using System.Security.Claims;
using AtlanticCity.Control.Api.Contracts;
using AtlanticCity.Control.Api.Security;
using AtlanticCity.Control.Application.Loads.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlanticCity.Control.Application.Loads.Queries.Detail;
using AtlanticCity.Control.Application.Loads.Queries.List;
using AtlanticCity.BuildingBlocks.Observability;

namespace AtlanticCity.Control.Api.Controllers;

[ApiController]
[Route("api/loads")]
public sealed class LoadsController(
    CreateLoadCommandHandler createLoadHandler,
    ListLoadsQueryHandler listLoadsHandler,
    GetLoadDetailQueryHandler getLoadDetailHandler)
    : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(
        Policy =
            ControlAuthorizationPolicies.MassLoadExecute)]
    public async Task<IActionResult> UploadAsync(
        [FromForm] UploadLoadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return this.AtlanticCityProblem(
                StatusCodes.Status400BadRequest,
                "Archivo requerido",
                "Debe seleccionar un archivo Excel.",
                "FILE_REQUIRED");
        }

        var userId =
            User.FindFirstValue(
                "sub");

        var userEmail =
            User.FindFirstValue(
                "email");

        if (string.IsNullOrWhiteSpace(
                userId) ||
            string.IsNullOrWhiteSpace(
                userEmail))
        {
            return this.AtlanticCityProblem(
                StatusCodes.Status401Unauthorized,
                "Usuario no autenticado",
                "No se pudo obtener el contexto del usuario autenticado.",
                "INVALID_USER_CONTEXT");
        }

        var correlationId =
            HttpContext.TraceIdentifier;

        await using var content =
            request.File.OpenReadStream();

        var command =
            new CreateLoadCommand(
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                content,
                userId,
                userEmail,
                correlationId);

        var result =
            await createLoadHandler.HandleAsync(
                command,
                cancellationToken);

        if (!result.IsAccepted)
        {
            var statusCode =
                result.ErrorCode ==
                "FILE_TOO_LARGE"
                    ? StatusCodes
                        .Status413PayloadTooLarge
                    : StatusCodes
                        .Status400BadRequest;

            var title =
                result.ErrorCode switch
                {
                    "FILE_TOO_LARGE" =>
                        "Archivo demasiado grande",

                    "INVALID_FILE_EXTENSION" =>
                        "Tipo de archivo inválido",

                    "EMPTY_FILE" =>
                        "Archivo vacío",

                    "FILE_REQUIRED" =>
                        "Archivo requerido",

                    _ =>
                        "Archivo inválido"
                };

            return this.AtlanticCityProblem(
                statusCode,
                title,
                result.ErrorMessage ??
                    "El archivo enviado no es válido.",
                result.ErrorCode ??
                    "INVALID_FILE");
        }

        return Accepted(
            new
            {
                loadId =
                    result.LoadId,

                status =
                    "Pending",

                correlationId
            });
    }

    [HttpGet]
    [Authorize(
        Policy =
            ControlAuthorizationPolicies.MassLoadRead)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? period = null,
        [FromQuery] string? status = null,
        [FromQuery] string? result = null,
        CancellationToken cancellationToken = default)
    {
        var response =
            await listLoadsHandler.HandleAsync(
                new ListLoadsQuery(
                    page,
                    pageSize,
                    period,
                    status,
                    result),
                cancellationToken);

        return Ok(
            response);
    }

    [HttpGet("{loadId:guid}")]
    [Authorize(
        Policy =
            ControlAuthorizationPolicies.MassLoadRead)]
    public async Task<IActionResult> GetByIdAsync(
        Guid loadId,
        CancellationToken cancellationToken)
    {
        var response =
            await getLoadDetailHandler.HandleAsync(
                new GetLoadDetailQuery(
                    loadId),
                cancellationToken);

        return response is null
            ? this.AtlanticCityProblem(
                StatusCodes.Status404NotFound,
                "Carga no encontrada",
                $"No se encontró la carga '{loadId}'.",
                "LOAD_NOT_FOUND")
            : Ok(response);
    }
}