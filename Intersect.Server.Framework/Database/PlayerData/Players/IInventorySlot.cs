using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IInventorySlot : IItem, ISlot, IPlayerOwned
    {
        Guid Id { get; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        int Slot { get; }
    }
}