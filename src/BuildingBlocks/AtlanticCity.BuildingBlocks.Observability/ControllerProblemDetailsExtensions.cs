using Microsoft.AspNetCore.Mvc;

namespace AtlanticCity.BuildingBlocks.Observability;

public static class
    ControllerProblemDetailsExtensions
{
    public static ObjectResult
        AtlanticCityProblem(
            this ControllerBase controller,
            int statusCode,
            string title,
            string detail,
            string code)
    {
        ArgumentNullException.ThrowIfNull(
            controller);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            detail);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            code);

        var problemDetails =
            new ProblemDetails
            {
                Status =
                    statusCode,

                Title =
                    title,

                Detail =
                    detail,

                Instance =
                    controller.HttpContext
                        .Request.Path
            };

        problemDetails.Extensions[
            "code"] =
            code;

        problemDetails.Extensions[
            "message"] =
            detail;

        problemDetails.Extensions[
            "correlationId"] =
            controller.HttpContext
                .TraceIdentifier;

        var result =
            new ObjectResult(
                problemDetails)
            {
                StatusCode =
                    statusCode
            };

        result.ContentTypes.Add(
            "application/problem+json");

        return result;
    }
}