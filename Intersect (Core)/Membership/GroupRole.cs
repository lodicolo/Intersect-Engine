using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Intersect.Membership
{
    public class GroupRole
    {
        public static GroupRole Owner => new GroupRole
        {
            Name = nameof(Owner),
            Permissions = new List<GroupPermission> {GroupPermission.Owner}
        };

        public static GroupRole Manager => new GroupRole
        {
            Name = nameof(Manager),
            Permissions = new List<GroupPermission>
            {
                GroupPermission.Update,
                GroupPermission.Invite,
                GroupPermission.Kick
            }
        };

        public Guid Id { get; private set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull] public List<GroupPermission> Permissions { get; private set; } = new List<GroupPermission>();

        public GroupRole()
        {
            Id = Guid.NewGuid();
            Name = Id.ToString();
        }

        public GroupRole(GroupRole other) : this()
        {

        }
    }
}
