using AtlanticCity.Control.Api.Security;
using AtlanticCity.Control.Application.Loads.Queries.Data;
using AtlanticCity.Control.Application.Loads.Queries.Errors;
using AtlanticCity.Control.Application.Loads.Queries.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlanticCity.BuildingBlocks.Observability;

namespace AtlanticCity.Control.Api.Controllers;

[ApiController]
[Route("api/loads/{loadId:guid}")]
[Authorize(
    Policy =
        ControlAuthorizationPolicies.MassLoadRead)]
public sealed class LoadResultsController(
    GetLoadHistoryQueryHandler historyHandler,
    GetLoadDataQueryHandler dataHandler,
    GetLoadErrorsQueryHandler errorsHandler)
    : ControllerBase
{
    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryAsync(
        Guid loadId,
        CancellationToken cancellationToken)
    {
        var result =
            await historyHandler.HandleAsync(
                new GetLoadHistoryQuery(
                    loadId),
                cancellationToken);

        return result is null
            ? LoadNotFound(loadId)
            : Ok(result);
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetDataAsync(
        Guid loadId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result =
            await dataHandler.HandleAsync(
                new GetLoadDataQuery(
                    loadId,
                    page,
                    pageSize),
                cancellationToken);

        return result is null
            ? LoadNotFound(loadId)
            : Ok(result);
    }

    [HttpGet("errors")]
    public async Task<IActionResult> GetErrorsAsync(
        Guid loadId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result =
            await errorsHandler.HandleAsync(
                new GetLoadErrorsQuery(
                    loadId,
                    page,
                    pageSize),
                cancellationToken);

        return result is null
            ? LoadNotFound(loadId)
            : Ok(result);
    }

    private ObjectResult LoadNotFound(
        Guid loadId)
    {
        return this.AtlanticCityProblem(
            StatusCodes.Status404NotFound,
            "Carga no encontrada",
            $"No se encontró la carga '{loadId}'.",
            "LOAD_NOT_FOUND");
    }
}