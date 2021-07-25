using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface ISpellSlot : ISpell, ISlot, IPlayerOwned
    {
        Guid Id { get; }
    }
}