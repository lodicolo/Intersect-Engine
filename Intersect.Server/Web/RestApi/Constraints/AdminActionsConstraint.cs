using Intersect.Enums;

namespace Intersect.Server.Web.RestApi.Constraints;

internal sealed class AdminActionsConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext httpContext,
        IRouter route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection
    )
    {
        if (!values.TryGetValue(routeKey, out var value) || value == null)
        {
            return false;
        }

        var stringValue = value as string ?? Convert.ToString(value);

        return Enum.TryParse<AdminAction>(stringValue, out _);
    }
}