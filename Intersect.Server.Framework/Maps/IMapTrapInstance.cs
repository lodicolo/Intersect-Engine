using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapTrapInstance
    {
        Guid Id { get; }

        void CheckEntityHasDetonatedTrap(IEntity entity);
        void Update();
    }
}