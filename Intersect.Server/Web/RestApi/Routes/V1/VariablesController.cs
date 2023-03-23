using System.Net;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Server.Database;
using Intersect.Server.Database.GameData;
using Intersect.Server.Entities;
using Intersect.Server.Web.RestApi.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes.V1;

[Route("variables")]
[Authorize]
public sealed class VariablesController : IntersectControllerBase
{
    [Route("global")]
    [HttpPost]
    public object GlobalVariablesGet([FromBody] PagingInfo pageInfo)
    {
        pageInfo.Page = Math.Max(pageInfo.Page, 0);
        pageInfo.Count = Math.Max(Math.Min(pageInfo.Count, 100), 5);

        var entries = GameContext.Queries.ServerVariables(pageInfo.Page, pageInfo.Count)?.ToList();

        return new
        {
            total = ServerVariableBase.Lookup.Count(),
            pageInfo.Page,
            count = entries?.Count ?? 0,
            entries
        };
    }

    [Route("global/{guid:guid}")]
    [HttpGet]
    public object GlobalVariableGet(Guid guid)
    {
        if (Guid.Empty == guid)
        {
            return CreateErrorResponse(HttpStatusCode.BadRequest, @"Invalid global variable id.");
        }

        var variable = GameContext.Queries.ServerVariableById(guid);

        if (variable == null)
        {
            return CreateErrorResponse(HttpStatusCode.NotFound, $@"No global variable with id '{guid}'.");
        }

        return variable;
    }

    [Route("global/{guid:guid}/value")]
    [HttpGet]
    public object GlobalVariableGetValue(Guid guid)
    {
        if (Guid.Empty == guid)
        {
            return CreateErrorResponse(HttpStatusCode.BadRequest, @"Invalid global variable id.");
        }

        var variable = GameContext.Queries.ServerVariableById(guid);

        if (variable == null)
        {
            return CreateErrorResponse(HttpStatusCode.NotFound, $@"No global variable with id '{guid}'.");
        }

        return new { value = variable?.Value.Value, };
    }

    [Route("global/{guid:guid}")]
    [HttpPost]
    public object GlobalVariableSet(Guid guid, [FromBody] VariableValuePayload valuePayload)
    {
        if (Guid.Empty == guid)
        {
            return CreateErrorResponse(HttpStatusCode.BadRequest, @"Invalid global variable id.");
        }

        var variable = GameContext.Queries.ServerVariableById(guid);

        if (variable == null)
        {
            return CreateErrorResponse(HttpStatusCode.NotFound, $@"No global variable with id '{guid}'.");
        }

        var changed = true;
        if (variable.Value?.Value == valuePayload.Value)
        {
            changed = false;
        }

        variable.Value.Value = valuePayload.Value;

        if (changed)
        {
            Player.StartCommonEventsWithTriggerForAll(CommonEventTrigger.ServerVariableChange, "", guid.ToString());
        }

        DbInterface.UpdatedServerVariables.AddOrUpdate(variable.Id, variable, (key, oldValue) => variable);

        return variable;
    }
}