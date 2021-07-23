using Intersect.Server.Framework.Maps;
using System;

namespace Intersect.Server.Maps
{

    public partial class MapItemSpawn : IMapItemSpawn
    {
        public Guid Id { get; } = Guid.NewGuid();

        public int AttributeSpawnX { get; set; } = -1;

        public int AttributeSpawnY { get; set; } = -1;

        public long RespawnTime { get; set; } = -1;

    }

}
