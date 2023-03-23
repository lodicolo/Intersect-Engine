using System.Net;
using System.Security.Claims;
using Intersect.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using IntersectUser = Intersect.Server.Database.PlayerData.User;

namespace Intersect.Server.Web.RestApi;

public abstract class IntersectControllerBase : ControllerBase
{
    public const int PAGE_SIZE_MAX = 100;

    public const int PAGE_SIZE_MIN = 5;

    // protected override void Initialize(HttpControllerContext controllerContext)
    // {
    //     base.Initialize(controllerContext);
    //     if (RequestContext != null && Configuration?.Formatters?.JsonFormatter?.SerializerSettings != null)
    //     {
    //         Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
    //             new ApiVisibilityContractResolver(RequestContext);
    //     }
    // }

    private IntersectUser _intersectUser;

    public IntersectUser IntersectUser
    {
        get
        {
            if (_intersectUser != default)
            {
                return _intersectUser;
            }

            var identity = User.Identity as ClaimsIdentity;
            var idString = identity?.FindFirst(IntersectClaimTypes.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(idString) || !Guid.TryParse(idString, out var id))
            {
                return default;
            }

            _intersectUser = IntersectUser.FindOnline(id) ?? IntersectUser.FindById(id);

            return _intersectUser;
        }
    }

    protected ObjectResult StatusCode(HttpStatusCode statusCode, object? value = default) =>
        StatusCode((int)statusCode, value);

    protected ObjectResult CreateErrorResponse(HttpStatusCode statusCode, string? message = default) =>
        StatusCode(statusCode, message);

    protected ObjectResult InternalServerError(string? message = default) =>
        CreateErrorResponse(HttpStatusCode.InternalServerError, message);
}