using System;

namespace Intersect.Groups
{
    /// <summary>
    /// Structure of group invitations.
    /// </summary>
    public class GroupInvite
    {
        public Guid Id { get; private set; }

        public Guid GroupId { get; set; }

        public string GroupName { get; set; }

        public Guid FromId { get; set; }

        public string FromName { get; set; }

        public Guid RoleId { get; set; }

        public long Expires { get; set; }

        public GroupInvite() : this(Guid.NewGuid()) { }

        public GroupInvite(Guid id) => Id = id;
    }
}
