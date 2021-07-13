using System;

using Intersect.GameObjects;
using Intersect.Server.Entities.Events;
using Intersect.Server.Framework.Entities;
using Intersect.Server.Framework.Maps;
using Intersect.Server.General;
using Intersect.Server.Maps;

namespace Intersect.Server.Classes.Maps
{

    public partial class MapTrapInstance : IMapTrapInstance
    {
        public Guid Id { get; } = Guid.NewGuid();

        private long Duration;

        public Guid MapId;

        public IEntity Owner;

        public SpellBase ParentSpell;

        public bool Triggered = false;

        public byte X;

        public byte Y;

        public byte Z;

        public MapTrapInstance(IEntity owner, SpellBase parentSpell, Guid mapId, byte x, byte y, byte z)
        {
            Owner = owner;
            ParentSpell = parentSpell;
            Duration = Globals.Timing.Milliseconds + ParentSpell.Combat.TrapDuration;
            MapId = mapId;
            X = x;
            Y = y;
            Z = z;
        }

        public void CheckEntityHasDetonatedTrap(IEntity entity)
        {
            if (!Triggered)
            {
                if (entity.MapId == MapId && entity.X == X && entity.Y == Y && entity.Z == Z)
                {
                    if (entity.GetType() == typeof(IPlayer) && Owner.GetType() == typeof(IPlayer))
                    {
                        //Don't detonate on yourself and party members on non-friendly spells!
                        if (Owner == entity || ((IPlayer)Owner).InParty((IPlayer)entity))
                        {
                            if (!ParentSpell.Combat.Friendly)
                            {
                                return;
                            }
                        }
                    }

                    if (entity is EventPageInstance)
                    {
                        return;
                    }

                    Owner.TryAttack(entity, ParentSpell, false, true);
                    Triggered = true;
                }
            }
        }

        public void Update()
        {
            if (Triggered)
            {
                MapInstance.Get(MapId).RemoveTrap(this);
            }

            if (Globals.Timing.Milliseconds > Duration)
            {
                MapInstance.Get(MapId).RemoveTrap(this);
            }
        }

    }

}
