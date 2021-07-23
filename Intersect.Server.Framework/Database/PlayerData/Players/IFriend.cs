using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IFriend
    {
        Guid Id { get; }
        IPlayer Owner { get; }
        IPlayer Target { get; }
    }
}