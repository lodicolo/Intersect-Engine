using Intersect.Enums;
using System;

namespace Intersect.Server.Database.Logging.Entities
{
    public interface IGuildHistory
    {
        string ActivityTypeName { get; }
        Guid GuildId { get; }
        Guid Id { get; }
        Guid InitiatorId { get; set; }
        string InitiatorName { get; set; }
        string Ip { get; set; }
        string Meta { get; set; }
        Guid PlayerId { get; set; }
        string PlayerName { get; set; }
        DateTime TimeStamp { get; set; }
        GuildActivityType Type { get; set; }
        Guid UserId { get; set; }
        string Username { get; set; }
    }
}