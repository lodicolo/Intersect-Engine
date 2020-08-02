using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intersect.Membership;

namespace Intersect.Server.Entities
{
    public partial class Player : IGroupMember
    {
        public virtual List<GroupMembership> GroupMemberships { get; private set; } = new List<GroupMembership>();

        [NotMapped] public List<Group> Groups => GroupMemberships.Select(membership => membership.Group).ToList();

        public bool IsInGroup(Guid groupId) => GroupMemberships.Exists(membership => membership.GroupId == groupId);

        public bool IsInGroup(Group group) => IsInGroup(group.Id);
    }
}
