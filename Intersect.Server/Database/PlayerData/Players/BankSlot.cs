using System;
using System.ComponentModel.DataAnnotations.Schema;

using Intersect.Server.Entities;
using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Intersect.Server.Database.PlayerData.Players
{

    public class BankSlot : Item, IBankSlot
    {

        public BankSlot()
        {
        }

        public BankSlot(int slot)
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
