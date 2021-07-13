using Intersect.GameObjects;

using System;

namespace Intersect.Server.Framework.Entities.Combat
{
    public partial interface IDoT
    {
        #region Properties

        IEntity Attacker { get; set; }

        int Count { get; set; }

        Guid Id { get; set; }

        SpellBase SpellBase { get; set; }

        IEntity Target { get; }

        #endregion Properties

        #region Methods

        bool CheckExpired();

        void Expire();

        void Tick();

        #endregion Methods
    }
}