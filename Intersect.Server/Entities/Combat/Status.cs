using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Server.Networking;
using Intersect.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersect.Server.Framework.Entities.Combat
{
    public partial class Status : IStatus
    {
        #region Fields

        private IEntity mEntity;

        #endregion Fields

        #region Constructors

        public Status(IEntity en, IEntity attacker, SpellBase spell, StatusTypes type, int duration, string data)
        {
            mEntity = en;
            Attacker = attacker;
            Spell = spell;
            Type = type;
            Data = data;

            // Handle Player specific stuff, such as interrupting spellcasts
            var tenacity = 0.0;
            if (en is IPlayer player)
            {
                // Get our player's Tenacity stat!
                if (!TenacityExcluded.Contains(type))
                {
                    tenacity = player.GetTenacity();
                }

                // Interrupt their spellcast if we are running a Silence, Sleep or Stun!
                if (InterruptStatusses.Contains(type))
                {
                    player.CastTime = 0;
                    player.CastTarget = null;
                    player.SpellCastSlot = -1;
                    PacketSender.SendEntityCancelCast(player);
                }
            }

            // Handle Npc specific stuff, such as loot tables!
            if (en is INpc npc)
            {
                // Add to loot map if not already eligible.
                if (!npc.LootMap.ContainsKey(Attacker.Id))
                {
                    npc.LootMap.TryAdd(Attacker.Id, true);
                    npc.LootMapCache = npc.LootMap.Keys.ToArray();
                }
            }

            if (type == StatusTypes.Shield)
            {
                for (var i = (int)Vitals.Health; i < (int)Vitals.VitalCount; i++)
                {
                    var vitalDiff = spell.Combat.VitalDiff[i];

                    Shield[i] = Math.Abs(vitalDiff) +
                                (int)(spell.Combat.Scaling * en.Stat[spell.Combat.ScalingStat].BaseStat / 100f);
                }
            }

            //If new Cleanse spell, remove all opposite statusses. (ie friendly dispels unfriendly and vice versa)
            if (Type == StatusTypes.Cleanse)
            {
                foreach (var status in en.CachedStatuses)
                {
                    if (spell.Combat.Friendly != status.Spell.Combat.Friendly)
                    {
                        status.RemoveStatus();
                    }
                }

                foreach (var dot in en.CachedDots)
                {
                    if (spell.Combat.Friendly != dot.SpellBase.Combat.Friendly)
                    {
                        dot.Expire();
                    }
                }
            }

            // Remove existing taunts if this is one and there are any others.
            // We'll be overwriting it, baby!
            if (Type == StatusTypes.Taunt)
            {
                foreach (var status in en.CachedStatuses)
                {
                    if (status.Type == StatusTypes.Taunt)
                    {
                        status.RemoveStatus();
                    }
                }
            }

            // Calculate our final duration and pass it on!
            var finalDuration = duration - (duration * (tenacity / 100f));
            if (en.Statuses.ContainsKey(spell))
            {
                en.Statuses[spell].StartTime = Timing.Global.Milliseconds;
                en.Statuses[spell].Duration = Timing.Global.Milliseconds + (long)finalDuration;
                en.Statuses[spell].StartTime = StartTime;
                en.CachedStatuses = en.Statuses.Values.ToArray();
            }
            else
            {
                StartTime = Timing.Global.Milliseconds;
                Duration = Timing.Global.Milliseconds + (long)finalDuration;
                en.Statuses.TryAdd(Spell, this);
                en.CachedStatuses = en.Statuses.Values.ToArray();
            }

            // If this is a taunt, force the target properly for players and NPCs
            if (Type == StatusTypes.Taunt)
            {
                // If player, force send target!
                if (en is IPlayer targetPlayer)
                {
                    en.Target = Attacker;
                    PacketSender.SetPlayerTarget(targetPlayer, Attacker.Id);
                }
                // If NPC, force assign target and make sure we have the highest threat +1 so we in theory have the highest threat.. for now..
                else if (en is INpc targetNpc)
                {
                    targetNpc.AssignTarget(Attacker);

                    if (!targetNpc.DamageMap.ContainsKey(Attacker))
                    {
                        if (targetNpc.DamageMap.Count > 0)
                        {
                            targetNpc.DamageMap.TryAdd(Attacker, targetNpc.DamageMap.ToArray().Max(x => x.Value) + 1);
                        }
                        else
                        {
                            targetNpc.DamageMap.TryAdd(Attacker, 1);
                        }
                    }
                    else
                    {
                        targetNpc.DamageMap[Attacker] = targetNpc.DamageMap.ToArray().Max(x => x.Value) + 1;
                    }
                }
                else
                {
                    en.Target = Attacker;
                }
            }
        }

        #endregion Constructors

        #region Properties

        public static List<StatusTypes> InterruptStatusses { get; set; } = new List<StatusTypes>()
        {
            StatusTypes.Silence,
            StatusTypes.Sleep,
            StatusTypes.Stun,
        };

        public static List<StatusTypes> TenacityExcluded { get; set; } = new List<StatusTypes>()
        {
            StatusTypes.None,
            StatusTypes.Stealth,
            StatusTypes.Cleanse,
            StatusTypes.Invulnerable,
            StatusTypes.OnHit,
            StatusTypes.Shield,
            StatusTypes.Transform,
        };

        public IEntity Attacker { get; set; }

        public string Data { get; set; } = "";

        public long Duration { get; set; }

        public int[] Shield { get; set; } = new int[(int)Vitals.VitalCount];

        public SpellBase Spell { get; set; }

        public long StartTime { get; set; }

        public StatusTypes Type { get; set; }

        #endregion Properties

        #region Methods

        public void DamageShield(Vitals vital, ref int amount)
        {
            if (Type == StatusTypes.Shield)
            {
                Shield[(int)vital] -= amount;
                if (Shield[(int)vital] <= 0)
                {
                    amount = -Shield[(int)vital]; //Return piercing damage.
                    Shield[(int)vital] = 0;
                    TryRemoveStatus();
                }
                else
                {
                    amount = 0; //Sheild is stronger than the damage dealt, so no piercing damage.
                }
            }
        }

        public void RemoveStatus()
        {
            mEntity.Statuses.TryRemove(Spell, out IStatus val);
            mEntity.CachedStatuses = mEntity.Statuses.Values.ToArray();

            // if this was a taunt status being removed, we have to scan for a new target!
            if (mEntity is INpc npc && Type == StatusTypes.Taunt)
            {
                npc.TryFindNewTarget(0, Guid.Empty, true);
            }
        }

        public void TryRemoveStatus()
        {
            if (Duration <= Timing.Global.Milliseconds) //Check the timer
            {
                RemoveStatus();
            }

            //If shield check for out of hp
            if (Type == StatusTypes.Shield)
            {
                for (var i = (int)Vitals.Health; i < (int)Vitals.VitalCount; i++)
                {
                    if (Shield[i] > 0)
                    {
                        return;
                    }
                }

                RemoveStatus();
            }
        }

        #endregion Methods
    }
}