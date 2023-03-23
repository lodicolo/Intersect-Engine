using System.Text;
using Intersect.Enums;
using Intersect.Server.General;
using Intersect.Server.Metrics;
using Intersect.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes.V1;

[ApiController]
[Authorize]
[Route("info")]
public sealed class InfoController : ControllerBase
{
    [Route("authorized")]
    [HttpGet]
    [Authorize]
    public object Authorized() => new
    {
        authorized = true
    };

    [Route("")]
    [HttpGet]
    public object Default() => new
    {
        name = Options.Instance.GameName,
        port = Options.ServerPort
    };

    [Route("config")]
    [HttpGet]
    public object Config() => Options.Instance;

    [Route("config/stats")]
    [HttpGet]
    public object CombatStats() => Enum.GetValues(typeof(Stat))
        .OfType<Stat>()
        .Where(value => value != Stat.StatCount)
        .Select(value => value.ToString())
        .ToArray();

    [Route("stats")]
    [HttpGet]
    public object Stats() => new
    {
        uptime = Timing.Global.Milliseconds,
        cps = Globals.Cps,
        connectedClients = Globals.Clients?.Count,
        onlineCount = Globals.OnlineList?.Count
    };

    [Route("metrics")]
    [HttpGet]
    public object StatsMetrics() => new HttpResponseMessage
    {
        Content = new StringContent(MetricsRoot.Instance.Metrics, Encoding.UTF8, "application/json")
    };
}