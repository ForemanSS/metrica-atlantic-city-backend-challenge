using AtlanticCity.Authentication.Domain.Users;

namespace AtlanticCity.Authentication.Application.Security;

public static class PermissionCatalog
{
    public const string MassLoadExecute =
        "mass-load.execute";

    public const string MassLoadRead =
        "mass-load.read";

    public static IReadOnlyCollection<string> ForRole(
        UserRole role)
    {
        return role switch
        {
            UserRole.Admin =>
            [
                MassLoadExecute,
                MassLoadRead
            ],

            UserRole.Operator =>
            [
                MassLoadExecute,
                MassLoadRead
            ],

            UserRole.Viewer =>
            [
                MassLoadRead
            ],

            _ => []
        };
    }
}