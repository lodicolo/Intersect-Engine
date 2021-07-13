using Intersect.GameObjects;
using Intersect.Server.Framework.Entities.Combat;

namespace Intersect.Server.Entities.Combat
{
    public partial class Buff : IBuff
    {
        #region Constructors

        public Buff(SpellBase spell, int flatStats, int percentageStats, long expireTime)
        {
            Spell = spell;
            FlatStatcount = flatStats;
            PercentageStatcount = percentageStats;
            ExpireTime = expireTime;
        }

        #endregion Constructors

        #region Properties

        public long ExpireTime { get; set; }

        public int FlatStatcount { get; set; }

        public int PercentageStatcount { get; set; }

        public SpellBase Spell { get; set; }

        #endregion Properties
    }
}