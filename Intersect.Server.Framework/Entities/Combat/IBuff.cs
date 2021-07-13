using Intersect.GameObjects;

namespace Intersect.Server.Framework.Entities.Combat
{
    public partial interface IBuff
    {
        #region Properties

        long ExpireTime { get; set; }

        int FlatStatcount { get; set; }

        int PercentageStatcount { get; set; }

        SpellBase Spell { get; set; }

        #endregion Properties
    }
}