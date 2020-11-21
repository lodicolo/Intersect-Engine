using System;

using Intersect.GameObjects;
using Intersect.Server.Database.PlayerData;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Intersect.Server.Entities
{
    public partial class Player
    {
        public Group Party => Groups.FirstOrDefault(group => group.GroupType.SpecialType == GroupSpecialType.Party);

        [JsonIgnore]
        public virtual List<Group> Groups => GroupMemberships.Select(membership => membership.Group).ToList();

        [JsonIgnore] public virtual List<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();

        [NotMapped] public List<GroupInvite> GroupInvites { get; } = new List<GroupInvite>();

        public bool TryFindMembership(Guid groupTypeId, out GroupMembership groupMembership) =>
            (groupMembership = GroupMemberships.Find(membership => membership.Group.GroupTypeId == groupTypeId)) != null;

        public bool TryFindMembership(GroupTypeDescriptor groupType, out GroupMembership groupMembership)
        {
            if (groupType == null)
            {
                throw new ArgumentNullException(nameof(groupType));
            }

            return TryFindMembership(groupType.Id, out groupMembership);
        }

        public bool InGroupWith(Guid groupTypeId, Player other) => other.TryFindMembership(groupTypeId, out _);

        public bool InGroupWith(GroupTypeDescriptor groupTypeDescriptor, Player other) =>
            InGroupWith(groupTypeDescriptor.Id, other);
    }
}
