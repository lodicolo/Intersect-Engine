using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes.V1;

[Route("demo")]
[Authorize]
public sealed class DemoController : IntersectControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public string Default() => "GET:demo";

    [HttpGet("authorize")]
    public object Authorize() => "GET:demo/authorize";
}