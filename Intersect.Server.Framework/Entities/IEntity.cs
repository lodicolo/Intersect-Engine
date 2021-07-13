using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Events;
using Intersect.Network.Packets.Server;
using Intersect.Server.Framework.Entities.Combat;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities
{
    public interface IEntity
    {
        #region Properties

        List<Guid> Animations { get; set; }

        long AttackTimer { get; set; }

        int[] BaseStats { get; set; }

        bool Blocking { get; set; }

        IDoT[] CachedDots { get; set; }

        IStatus[] CachedStatuses { get; set; }

        IEntity CastTarget { get; set; }

        long CastTime { get; set; }

        Guid CollisionIndex { get; set; }

        Color Color { get; set; }

        long CombatTimer { get; set; }

        bool Dead { get; set; }

        int Dir { get; set; }

        ConcurrentDictionary<Guid, IDoT> DoT { get; set; }

        object EntityLock { get; }

        string Face { get; set; }

        Label FooterLabel { get; set; }

        string FooterLabelJson { get; set; }

        Label HeaderLabel { get; set; }

        string HeaderLabelJson { get; set; }

        bool HideEntity { get; set; }

        bool HideName { get; set; }

        Guid Id { get; set; }

        bool IsDisposed { get; }

        List<InventorySlot> Items { get; set; }

        string JsonColor { get; set; }

        int Level { get; set; }

        MapInstance Map { get; }

        Guid MapId { get; set; }

        string MapName { get; }

        EventMoveRoute MoveRoute { get; set; }

        EventPageInstance MoveRouteSetter { get; set; }

        long MoveTimer { get; set; }

        string Name { get; set; }

        Color NameColor { get; set; }

        string NameColorJson { get; set; }

        bool Passable { get; set; }

        long RegenTimer { get; set; }

        int SpellCastSlot { get; set; }

        string SpellCooldownsJson { get; set; }

        List<SpellSlot> Spells { get; set; }

        string Sprite { get; set; }

        int[] StatPointAllocations { get; set; }

        string StatPointsJson { get; set; }

        string StatsJson { get; set; }

        ConcurrentDictionary<SpellBase, IStatus> Statuses { get; }

        bool StatusesUpdated { get; set; }

        IStat[] Stat { get; set; }

        IEntity Target { get; set; }

        string VitalsJson { get; set; }

        bool VitalsUpdated { get; set; }

        int X { get; set; }

        int Y { get; set; }

        int Z { get; set; }

        #endregion Properties

        #region Methods

        void AddVital(Vitals vital, int amount);

        void Attack(IEntity enemy, int baseDamage, int secondaryDamage, DamageType damageType, Stats scalingStat, int scaling, int critChance, double critMultiplier, List<KeyValuePair<Guid, sbyte>> deadAnimations = null, List<KeyValuePair<Guid, sbyte>> aliveAnimations = null, bool isAutoAttack = false);

        int CalculateAttackTime();

        bool CanAttack(IEntity entity, SpellBase spell);

        bool CanCastSpell(Guid spellId, IEntity target);

        bool CanCastSpell(SpellBase spellDescriptor, IEntity target);

        int CanMove(int moveDir);

        void CastSpell(Guid spellId, int spellSlot = -1);

        void ChangeDir(int dir);

        void Die(bool dropItems = true, IEntity killer = null);

        void Dispose();

        EntityPacket EntityPacket(EntityPacket packet = null, IPlayer forPlayer = null);

        int GetDirectionTo(IEntity target);

        int GetDistanceBetween(MapInstance mapA, MapInstance mapB, int xTileA, int xTileB, int yTileA, int yTileB);

        int GetDistanceTo(IEntity target);

        int GetDistanceTo(MapInstance targetMap, int targetX, int targetY);

        EntityTypes GetEntityType();

        int GetMaxVital(int vital);

        int GetMaxVital(Vitals vital);

        int[] GetMaxVitals();

        float GetMovementTime();

        int[] GetStatValues();

        int GetVital(int vital);

        int GetVital(Vitals vital);

        int[] GetVitals();

        int GetWeaponDamage();

        bool HasVital(Vitals vital);

        bool InRangeOf(IEntity target, int range);

        bool IsAllyOf(IEntity otherEntity);

        bool IsDead();

        bool IsFullVital(Vitals vital);

        bool IsPassable();

        void KilledEntity(IEntity entity);

        void Move(int moveDir, IPlayer forPlayer, bool doNotUpdate = false, bool correction = false);

        void NotifySwarm(IEntity attacker);

        void ProcessRegen();

        void Reset();

        void RestoreVital(Vitals vital);

        void SetMaxVital(int vital, int value);

        void SetMaxVital(Vitals vital, int value);

        void SetVital(int vital, int value);

        void SetVital(Vitals vital, int value);

        StatusPacket[] StatusPackets();

        void SubVital(Vitals vital, int amount);

        void TryAttack(IEntity target);

        void TryAttack(IEntity target, int baseDamage, DamageType damageType, Stats scalingStat, int scaling, int critChance, double critMultiplier, List<KeyValuePair<Guid, sbyte>> deadAnimations = null, List<KeyValuePair<Guid, sbyte>> aliveAnimations = null, ItemBase weapon = null);

        void TryAttack(IEntity target, ProjectileBase projectile, SpellBase parentSpell, ItemBase parentItem, byte projectileDir);

        void TryAttack(IEntity target, SpellBase spellBase, bool onHitTrigger = false, bool trapTrigger = false);

        void TryBlock(bool blocking);

        bool TryToChangeDimension();

        void Update(long timeMs);

        void Warp(Guid newMapId, float newX, float newY, bool adminWarp = false);

        void Warp(Guid newMapId, float newX, float newY, byte newDir, bool adminWarp = false, byte zOverride = 0, bool mapSave = false);

        #endregion Methods
    }
}