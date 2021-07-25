using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Database.PlayerData.Players;
using System;

namespace Intersect.Server.Database.PlayerData.Players
{
    public interface IGuildBankSlot : IItem, ISlot
    {
        IGuild Guild { get; }
        Guid GuildId { get; }
        Guid Id { get; }
    }
}