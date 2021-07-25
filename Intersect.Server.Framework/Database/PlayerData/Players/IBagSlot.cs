using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IBagSlot : ISlot, IItem
    {
        Guid Id { get; }
        IBag ParentBag { get; }
        Guid ParentBagId { get; }
    }
}