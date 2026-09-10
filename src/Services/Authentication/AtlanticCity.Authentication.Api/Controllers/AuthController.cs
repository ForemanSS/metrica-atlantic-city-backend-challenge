using AtlanticCity.Authentication.Api.Contracts;
using AtlanticCity.Authentication.Application.Login;
using AtlanticCity.Authentication.Application.RefreshTokens;
using AtlanticCity.Authentication.Application.RevokeToken;
using AtlanticCity.BuildingBlocks.Observability;
using Microsoft.AspNetCore.Mvc;

namespace AtlanticCity.Authentication.Api.Controllers;
[ApiController]
[Route("auth")]
public sealed class AuthController(
    LoginCommandHandler loginHandler,
    RefreshTokenCommandHandler refreshTokenHandler,
    RevokeTokenCommandHandler revokeTokenHandler)
    : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new LoginCommand(
                request.Email,
                request.Password,
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString());

        var result =
            await loginHandler.HandleAsync(
                command,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return this.AtlanticCityProblem(
                StatusCodes.Status401Unauthorized,
                "Credenciales inválidas",
                "El correo o la contraseña no son válidos.",
                "INVALID_CREDENTIALS");
        }

        return Ok(
            new LoginResponse(
                result.AccessToken!,
                result.RefreshToken!,
                "Bearer",
                result.ExpiresAt!.Value,
                new AuthenticatedUserResponse(
                    result.UserId!.Value,
                    result.Email!,
                    result.DisplayName!,
                    result.Role!,
                    result.Permissions)));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<TokenResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new RefreshTokenCommand(
                request.RefreshToken,
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString());

        var result =
            await refreshTokenHandler.HandleAsync(
                command,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return this.AtlanticCityProblem(
                StatusCodes.Status401Unauthorized,
                "Refresh token inválido",
                "El refresh token es inválido o ha expirado.",
                "INVALID_REFRESH_TOKEN");
        }

        return Ok(
            new TokenResponse(
                result.AccessToken!,
                result.RefreshToken!,
                "Bearer",
                result.ExpiresAt!.Value));
    }

    [HttpPost("revoke")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new RevokeTokenCommand(
                request.RefreshToken,
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString());

        await revokeTokenHandler.HandleAsync(
            command,
            cancellationToken);

        return NoContent();
    }
}