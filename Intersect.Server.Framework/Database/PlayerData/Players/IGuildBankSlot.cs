using System;

namespace Intersect.Server.Database.PlayerData.Players
{
    public interface IGuildBankSlot
    {
        Guild Guild { get; }
        Guid GuildId { get; }
        Guid Id { get; }
        int Slot { get; }
    }
}