using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Network.Packets.Server;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities
{
    public interface IProjectile : IEntity
    {
        ProjectileBase Base { get; set; }
        bool HasGrappled { get; set; }
        ItemBase Item { get; set; }
        IEntity Owner { get; set; }
        IProjectileSpawn[] Spawns { get; set; }
        SpellBase Spell { get; set; }
        IEntity Target { get; set; }

        bool CheckForCollision(IProjectileSpawn spawn);
        void Die(bool dropItems = true, IEntity killer = null);
        EntityPacket EntityPacket(EntityPacket packet = null, IPlayer forPlayer = null);
        EntityTypes GetEntityType();
        bool MoveFragment(IProjectileSpawn spawn, bool move = true);
        void ProcessFragments(List<Guid> projDeaths, List<KeyValuePair<Guid, int>> spawnDeaths);
        void Update(List<Guid> projDeaths, List<KeyValuePair<Guid, int>> spawnDeaths);
    }
}