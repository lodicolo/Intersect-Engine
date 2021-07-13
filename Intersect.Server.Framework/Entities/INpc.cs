using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Network.Packets.Server;

using System;
using System.Collections.Concurrent;

namespace Intersect.Server.Framework.Entities
{
    public interface INpc : IEntity
    {
        #region Properties

        /// <summary>
        /// The map on which this NPC was "aggro'd" and started chasing a target.
        /// </summary>
        MapInstance AggroCenterMap { get; set; }

        /// <summary>
        /// The X value on which this NPC was "aggro'd" and started chasing a target.
        /// </summary>
        int AggroCenterX { get; set; }

        /// <summary>
        /// The Y value on which this NPC was "aggro'd" and started chasing a target.
        /// </summary>
        int AggroCenterY { get; set; }

        /// <summary>
        /// The Z value on which this NPC was "aggro'd" and started chasing a target.
        /// </summary>
        int AggroCenterZ { get; set; }

        NpcBase Base { get; }

        long CastFreq { get; set; }

        ConcurrentDictionary<IEntity, long> DamageMap { get; set; }

        IEntity DamageMapHighest { get; }

        bool Despawnable { get; set; }

        int FindTargetDelay { get; set; }

        long FindTargetWaitTime { get; set; }

        //Moving
        long LastRandomMove { get; set; }

        ConcurrentDictionary<Guid, bool> LootMap { get; set; }

        Guid[] LootMapCache { get; set; }

        byte Range { get; set; }

        //Respawn/Despawn
        long RespawnTime { get; set; }

        #endregion Properties

        #region Methods

        void AssignTarget(IEntity en);

        int CalculateAttackTime();

        bool CanAttack(IEntity entity, SpellBase spell);

        int CanMove(int moveDir);

        bool CanNpcCombat(IEntity enemy, bool friendly = false);

        bool CanPlayerAttack(IPlayer en);

        void Die(bool generateLoot = true, IEntity killer = null);

        EntityPacket EntityPacket(EntityPacket packet = null, IPlayer forPlayer = null);

        int GetAggression(IPlayer player);

        EntityTypes GetEntityType();

        bool IsAllyOf(IEntity otherEntity);

        bool IsFleeing();

        void NotifySwarm(IEntity attacker);

        void ProcessRegen();

        void RemoveFromDamageMap(IEntity en);

        void RemoveTarget();

        bool ShouldAttackPlayerOnSight(IPlayer en);

        bool TargetHasStealth(IEntity target);

        void TryAttack(IEntity target);

        void TryFindNewTarget(long timeMs, Guid avoidId = default, bool ignoreTimer = false, IEntity attackedBy = null);

        void Update(long timeMs);

        void Warp(Guid newMapId, float newX, float newY, byte newDir, bool adminWarp = false, byte zOverride = 0, bool mapSave = false);

        #endregion Methods
    }
}