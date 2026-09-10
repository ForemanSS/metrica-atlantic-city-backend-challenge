namespace AtlanticCity.Authentication.Application.Security;

public interface IRefreshTokenService
{
    RefreshTokenValue Generate();

    string Hash(string token);
}