using System;
using System.ComponentModel.DataAnnotations.Schema;

using Intersect.Server.Entities;
using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Intersect.Server.Database.PlayerData.Players
{

    public class Friend : IFriend
    {

        public Friend()
        {
        }

        public Friend(IPlayer me, IPlayer friend)
        {
            Owner = me;
            Target = friend;
        }

        [JsonProperty(nameof(Owner))]

        // Note: Do not change to OwnerId or it collides with the hidden
        // one that Entity Framework expects/creates under the covers.
        private Guid JsonOwnerId => Owner?.Id ?? Guid.Empty;

        [JsonProperty(nameof(Target))]

        // Note: Do not change to TargetId or it collides with the hidden
        // one that Entity Framework expects/creates under the covers.
        private Guid JsonTargetId => Target?.Id ?? Guid.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        [JsonIgnore]
        public virtual IPlayer Owner { get; private set; }

        [JsonIgnore]
        public virtual IPlayer Target { get; private set; }

    }

}
