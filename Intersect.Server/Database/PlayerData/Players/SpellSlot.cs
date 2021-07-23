using System;
using System.ComponentModel.DataAnnotations.Schema;
using Intersect.Server.Database;
using Intersect.Server.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Intersect.Server.Framework.Database.PlayerData.Players
{

    public class SpellSlot : Spell, ISpellSlot
    {

        public SpellSlot()
        {
        }

        public SpellSlot(int slot)
        {
            Slot = slot;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity), JsonIgnore]
        public Guid Id { get; private set; }

        [JsonIgnore]
        public Guid PlayerId { get; private set; }

        [JsonIgnore]
        public virtual IPlayer Player { get; private set; }

        public int Slot { get; private set; }

    }

}
