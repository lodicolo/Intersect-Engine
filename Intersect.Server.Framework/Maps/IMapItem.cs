using Intersect.Server.Framework.Database;
using System;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapItem : IItem
    {
        int AttributeSpawnX { get; set; }
        int AttributeSpawnY { get; set; }
        long DespawnTime { get; set; }
        int TileIndex { get; }
        Guid UniqueId { get; }
        int X { get; }
        int Y { get; }
        Guid Owner { get; set; }
        long OwnershipTime { get; set; }
        bool VisibleToAll { get; set; }

        string Data();
        void SetupStatBuffs(IItem item);
    }
}