using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intersect.Server.Database.PlayerData;

using Newtonsoft.Json;

namespace Intersect.Server.Entities
{
    public partial class Player
    {

        [JsonIgnore]
        public virtual List<Group> Groups => GroupMemberships.Select(membership => membership.Group).ToList();

        [JsonIgnore] public virtual List<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();
    }
}
