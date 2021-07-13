using Intersect.Enums;
using Intersect.GameObjects;

namespace Intersect.Server.Framework.Entities.Combat
{
    public partial interface IStatus
    {
        #region Properties

        IEntity Attacker { get; set; }

        string Data { get; set; }

        long Duration { get; set; }

        int[] Shield { get; set; }

        SpellBase Spell { get; set; }

        long StartTime { get; set; }

        StatusTypes Type { get; set; }

        #endregion Properties

        #region Methods

        void DamageShield(Vitals vital, ref int amount);

        void RemoveStatus();

        void TryRemoveStatus();

        #endregion Methods
    }
}