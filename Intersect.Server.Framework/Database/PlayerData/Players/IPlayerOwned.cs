using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{

    public interface IPlayerOwned
    {

        IPlayer Player { get; }

        Guid PlayerId { get; }

    }

}
