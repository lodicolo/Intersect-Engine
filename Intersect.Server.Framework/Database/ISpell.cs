using Intersect.Server.Database;
using System;

namespace Intersect.Server.Framework.Database
{
    public interface ISpell
    {
        Guid SpellId { get; set; }
        string SpellName { get; }

        ISpell Clone();
        void Set(ISpell spell);
    }
}