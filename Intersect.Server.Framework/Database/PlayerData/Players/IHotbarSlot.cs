using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IHotbarSlot : ISlot, IPlayerOwned
    {
        Guid BagId { get; set; }
        Guid Id { get; }
        Guid ItemOrSpellId { get; set; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        int[] PreferredStatBuffs { get; set; }
        int Slot { get; }
        string StatBuffsJson { get; set; }

        string Data();
    }
}