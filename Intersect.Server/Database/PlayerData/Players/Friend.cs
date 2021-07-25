using System;
using System.ComponentModel.DataAnnotations.Schema;

using Intersect.Extensions;
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
            
            Owner = me as Player ??
                    throw new ArgumentException(
                        ExceptionMessages.ExpectedReceived.Format(typeof(Player).FullName, me?.GetType().FullName), nameof(me)
                    );
            Target = friend as Player ??
                     throw new ArgumentException(
                         ExceptionMessages.ExpectedReceived.Format(typeof(Player).FullName, friend?.GetType().FullName), nameof(friend)
                     );
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
        public virtual Player Owner { get; private set; }

        [JsonIgnore, NotMapped]
        IPlayer IFriend.Owner => Owner;

        [JsonIgnore]
        public virtual Player Target { get; private set; }

        [JsonIgnore, NotMapped]
        IPlayer IFriend.Target => Target;

    }

}
