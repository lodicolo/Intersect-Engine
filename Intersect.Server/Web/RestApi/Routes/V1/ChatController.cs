using System.Net;
using Intersect.Enums;
using Intersect.Server.General;
using Intersect.Server.Networking;
using Intersect.Server.Web.RestApi.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes.V1;

[ApiController]
[Route("chat")]
[Authorize]
public sealed class ChatController : IntersectControllerBase
{
    [Route("")]
    [Route("global")]
    [HttpPost]
    public object SendGlobal([FromBody] ChatMessage chatMessage)
    {
        try
        {
            PacketSender.SendGlobalMsg(
                chatMessage.Message,
                chatMessage.Color ?? CustomColors.Chat.AnnouncementChat,
                chatMessage.Target
            );

            return new
            {
                success = true,
                chatMessage
            };
        }
        catch (Exception exception)
        {
            return CreateErrorResponse(HttpStatusCode.InternalServerError, exception.Message);
        }
    }

    [Route("direct/{lookupKey:LookupKey}")]
    [HttpPost]
    public object SendDirect(LookupKey lookupKey, [FromBody] ChatMessage chatMessage)
    {
        if (lookupKey.IsInvalid)
        {
            return CreateErrorResponse(
                HttpStatusCode.BadRequest,
                lookupKey.IsIdInvalid ? @"Invalid player id." : @"Invalid player name."
            );
        }

        var client = Globals.Clients.Find(
            lookupClient => string.Equals(
                lookupKey.Name,
                lookupClient?.Entity?.Name,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (client == null)
        {
            return CreateErrorResponse(HttpStatusCode.NotFound, $@"No player found for '{lookupKey}'.");
        }

        try
        {
            PacketSender.SendChatMsg(
                client.Entity,
                chatMessage.Message,
                ChatMessageType.PM,
                chatMessage.Color ?? CustomColors.Chat.PlayerMsg,
                chatMessage.Target
            );

            return new
            {
                success = true,
                lookupKey,
                chatMessage
            };
        }
        catch (Exception exception)
        {
            return CreateErrorResponse(HttpStatusCode.InternalServerError, exception.Message);
        }
    }

    [Route("proximity/{mapId:guid}")]
    [HttpPost]
    public object SendProximity(Guid mapId, [FromBody] ChatMessage chatMessage)
    {
        if (Guid.Empty == mapId)
        {
            return CreateErrorResponse(HttpStatusCode.BadRequest, $@"Invalid map id '{mapId}'.");
        }

        try
        {
            if (PacketSender.SendProximityMsg(
                    chatMessage.Message,
                    ChatMessageType.Local,
                    mapId,
                    chatMessage.Color ?? CustomColors.Chat.ProximityMsg,
                    chatMessage.Target
                ))
            {
                return new
                {
                    success = true,
                    mapId,
                    chatMessage
                };
            }

            return CreateErrorResponse(HttpStatusCode.NotFound, $@"No map found for '{mapId}'.");
        }
        catch (Exception exception)
        {
            return CreateErrorResponse(HttpStatusCode.InternalServerError, exception.Message);
        }
    }

    // TODO: "party" message endpoint?
}