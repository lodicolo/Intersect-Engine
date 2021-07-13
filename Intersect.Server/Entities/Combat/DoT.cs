using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Server.Framework.Entities;
using Intersect.Server.Framework.Entities.Combat;
using Intersect.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersect.Server.Entities.Combat
{
    public partial class DoT : IDoT
    {
        #region Fields

        private long mInterval;

        #endregion Fields

        #region Constructors

        public DoT(IEntity attacker, Guid spellId, IEntity target)
        {
            SpellBase = SpellBase.Get(spellId);

            Attacker = attacker;
            Target = target;

            if (SpellBase == null || SpellBase.Combat.HotDotInterval < 1)
            {
                return;
            }

            // Does target have a cleanse buff? If so, do not allow this DoT when spell is unfriendly.
            if (!SpellBase.Combat.Friendly)
            {
                foreach (var status in Target.CachedStatuses)
                {
                    if (status.Type == StatusTypes.Cleanse)
                    {
                        return;
                    }
                }
            }

            mInterval = Timing.Global.Milliseconds + SpellBase.Combat.HotDotInterval;
            Count = SpellBase.Combat.Duration / SpellBase.Combat.HotDotInterval - 1;
            target.DoT.TryAdd(Id, this);
            target.CachedDots = target.DoT.Values.ToArray();

            //Subtract 1 since the first tick always occurs when the spell is cast.
        }

        #endregion Constructors

        #region Properties

        public IEntity Attacker { get; set; }

        public int Count { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();

        public SpellBase SpellBase { get; set; }

        public IEntity Target { get; }

        #endregion Properties

        #region Methods

        public bool CheckExpired()
        {
            if (Target != null && !Target.DoT.ContainsKey(Id))
            {
                return false;
            }

            if (SpellBase == null || Count > 0)
            {
                return false;
            }

            Expire();

            return true;
        }

        public void Expire()
        {
            if (Target != null)
            {
                Target.DoT?.TryRemove(Id, out DoT val);
                Target.CachedDots = Target.DoT?.Values.ToArray() ?? new DoT[0];
            }
        }

        public void Tick()
        {
            if (CheckExpired())
            {
                return;
            }

            if (mInterval > Timing.Global.Milliseconds)
            {
                return;
            }

            var deadAnimations = new List<KeyValuePair<Guid, sbyte>>();
            var aliveAnimations = new List<KeyValuePair<Guid, sbyte>>();
            if (SpellBase.HitAnimationId != Guid.Empty)
            {
                deadAnimations.Add(new KeyValuePair<Guid, sbyte>(SpellBase.HitAnimationId, (sbyte)Directions.Up));
                aliveAnimations.Add(new KeyValuePair<Guid, sbyte>(SpellBase.HitAnimationId, (sbyte)Directions.Up));
            }

            var damageHealth = SpellBase.Combat.VitalDiff[(int)Vitals.Health];
            var damageMana = SpellBase.Combat.VitalDiff[(int)Vitals.Mana];

            Attacker?.Attack(
                Target, damageHealth, damageMana,
                (DamageType)SpellBase.Combat.DamageType, (Stats)SpellBase.Combat.ScalingStat,
                SpellBase.Combat.Scaling, SpellBase.Combat.CritChance, SpellBase.Combat.CritMultiplier, deadAnimations,
                aliveAnimations, false
            );

            mInterval = Timing.Global.Milliseconds + SpellBase.Combat.HotDotInterval;
            Count--;
        }

        #endregion Methods
    }
}