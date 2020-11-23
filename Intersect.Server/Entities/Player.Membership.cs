using Intersect.GameObjects;
using Intersect.Server.Database.PlayerData.Groups;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Intersect.Groups;

namespace Intersect.Server.Entities
{
    public partial class Player
    {
        public Group Party => Groups.FirstOrDefault(group => group.GroupType.SpecialType == GroupSpecialType.Party);

        public bool InParty => Party != null;

        [JsonIgnore] public virtual List<Group> Groups => Memberships.Select(membership => membership.Group).ToList();

        [JsonIgnore] public virtual List<GroupMembership> Memberships { get; set; } = new List<GroupMembership>();

        [NotMapped] public List<GroupInvite> GroupInvites { get; } = new List<GroupInvite>();

        public bool TryFindInvite(Guid inviteId, out GroupInvite groupInvite) =>
            null != (groupInvite = GroupInvites.FirstOrDefault(invite => invite.Id == inviteId));

        public bool TryFindInviteForGroup(Guid groupId, out GroupInvite groupInvite) =>
            null != (groupInvite = GroupInvites.FirstOrDefault(invite => invite.GroupId == groupId));

        public bool TryFindMembership(Guid groupTypeId, out GroupMembership groupMembership) =>
            (groupMembership = Memberships.Find(membership => membership.Group.GroupTypeId == groupTypeId)) != null;

        public bool TryFindMembership(GroupTypeDescriptor groupType, out GroupMembership groupMembership)
        {
            if (groupType == null)
            {
                throw new ArgumentNullException(nameof(groupType));
            }

            return TryFindMembership(groupType.Id, out groupMembership);
        }

        public bool TryFindMembership(GroupSpecialType groupSpecialType, out GroupMembership groupMembership) =>
            (groupMembership =
                Memberships.Find(membership => membership.Group.GroupType.SpecialType == groupSpecialType)) !=
            null;

        public bool InGroupWith(Guid groupTypeId, Player other) =>
            other?.TryFindMembership(groupTypeId, out _) ?? throw new ArgumentNullException(nameof(other));

        public bool InGroupWith(GroupTypeDescriptor groupTypeDescriptor, Player other)
        {
            if (groupTypeDescriptor == null)
            {
                throw new ArgumentNullException(nameof(groupTypeDescriptor));
            }

            return InGroupWith(groupTypeDescriptor.Id, other);
        }
    }
}
