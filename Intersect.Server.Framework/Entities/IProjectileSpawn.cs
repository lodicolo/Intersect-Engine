using Intersect.GameObjects;
using System;

namespace Intersect.Server.Framework.Entities
{
    public interface IProjectileSpawn
    {
        bool Dead { get; set; }
        byte Dir { get; set; }
        int Distance { get; set; }
        Guid MapId { get; set; }
        IProjectile Parent { get; set; }
        ProjectileBase ProjectileBase { get; set; }
        long TransmittionTimer { get; set; }
        float X { get; set; }
        float Y { get; set; }
        byte Z { get; set; }

        bool HitEntity(IEntity en);
        bool IsAtLocation(Guid mapId, int x, int y, int z);
    }
}