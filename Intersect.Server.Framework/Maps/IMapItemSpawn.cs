using System;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapItemSpawn
    {
        int AttributeSpawnX { get; set; }
        int AttributeSpawnY { get; set; }
        Guid Id { get; }
        long RespawnTime { get; set; }
    }
}