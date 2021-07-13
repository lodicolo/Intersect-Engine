using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IBankSlot
    {
        Guid Id { get; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        int Slot { get; }
    }
}