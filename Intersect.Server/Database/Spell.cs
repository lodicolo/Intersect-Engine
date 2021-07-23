using System;
using System.ComponentModel.DataAnnotations.Schema;
using Intersect.GameObjects;
using Intersect.Server.Framework.Database;
using Newtonsoft.Json;

namespace Intersect.Server.Database
{

    public class Spell : ISpell
    {

        public Spell()
        {
        }

        public Spell(Guid spellId)
        {
            SpellId = spellId;
        }

        public Guid SpellId { get; set; }

        [NotMapped]
        public string SpellName => SpellBase.GetName(SpellId);

        //SpellCD NO LONGER USED
        //CAN'T REMOVE VIA EF UNTIL SQLITE ALLOWS ALTER TABLE DROP COLUMN
        //DON"T REMEMBER THIS VARIABLE ELSE EF WILL FAIL TO SAVE NEW PLAYERS
        [JsonIgnore]
        public long SpellCd { get; set; }

        public static Spell None => new Spell(Guid.Empty);

        public ISpell Clone()
        {
            var newSpell = new Spell() {
                SpellId = SpellId
            };

            return newSpell;
        }

        public virtual void Set(ISpell spell)
        {
            SpellId = spell.SpellId;
        }

    }

}
