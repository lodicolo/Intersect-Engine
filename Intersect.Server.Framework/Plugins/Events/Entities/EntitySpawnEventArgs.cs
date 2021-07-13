using Intersect.Server.Framework.Entities;

using System;

namespace Intersect.Server.Framework.Plugins.Events.Entities
{
    public class EntitySpawnEventArgs : EventArgs
    {
        public IEntity Entity { get; }
    }
}
