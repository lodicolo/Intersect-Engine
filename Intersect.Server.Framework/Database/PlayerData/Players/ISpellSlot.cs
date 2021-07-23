using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Database.PlayerData.Players
{
    public interface ISpellSlot : ISpell, ISlot, IPlayerOwned
    {
        Guid Id { get; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        int Slot { get; }
    }
}