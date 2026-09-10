using AtlanticCity.Authentication.Domain.Users;

namespace AtlanticCity.Authentication.Application.Security;

public interface IAccessTokenService
{
    AccessTokenResult Generate(
        User user,
        IReadOnlyCollection<string> permissions);
}