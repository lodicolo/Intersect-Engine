using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Newtonsoft.Json;

namespace Intersect.Membership
{
    public class GroupMembership
    {
        [ForeignKey(nameof(Group)), Column(Order = 0)]
        public Guid GroupId { get; private set; }

        public virtual Group Group { get; private set; }

        [ForeignKey(nameof(Member)), Column(Order = 1)]
        public Guid MemberId { get; private set; }

        public virtual IGroupMember Member { get; private set; }

        public List<Guid> RoleIds { get; private set; } = new List<Guid>();

        [NotMapped, JsonIgnore]
        public List<GroupRole> Roles => Group.Roles.Where(role => RoleIds.Contains(role.Id)).ToList();

        public List<GroupPermission> Permissions { get; private set; } = new List<GroupPermission>();
    }
}
