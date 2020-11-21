using Intersect.GameObjects;
using Intersect.Server.Entities;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Intersect.Server.General;

namespace Intersect.Server.Database.PlayerData
{
    public class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public Guid Id { get; private set; }

        public Guid GroupTypeId { get; set; }

        [JsonIgnore, NotMapped] public GroupTypeDescriptor GroupType => GroupTypeDescriptor.Get(GroupTypeId);

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Deleted { get; set; }

        [NotMapped] public List<GroupRole> Roles { get; set; } = new List<GroupRole>();

        [NotMapped] public List<GroupRoleDescriptor> RoleDescriptors => GroupType?.Roles;

        [JsonIgnore]
        public virtual List<Player> Members => Memberships.Select(membership => membership.Member).ToList();

        [JsonIgnore] public virtual List<GroupMembership> Memberships { get; set; } = new List<GroupMembership>();

        private GroupMembership FindMembership(Player member) =>
            Memberships.FirstOrDefault(membership => membership.MemberId == member.Id);

        public GroupRole FindRole(string name) =>
            Roles.Find(role => string.Equals(role.DescriptorName, name, StringComparison.Ordinal));

        public GroupRoleDescriptor FindRoleDescriptor(string name) =>
            RoleDescriptors?.Find(roleDescriptor => string.Equals(roleDescriptor.Name, name, StringComparison.Ordinal));

        public int FindRoleIndex(string name) =>
            Roles.FindIndex(role => string.Equals(role.DescriptorName, name, StringComparison.Ordinal));

        public int FindRoleDescriptorIndex(string name) =>
            RoleDescriptors?.FindIndex(
                roleDescriptor => string.Equals(roleDescriptor.Name, name, StringComparison.Ordinal)
            ) ??
            -1;

        public bool HasMember(Player member) => FindMembership(member) != null;

        public bool AddMember(Player member, string groupRoleName) =>
            AddMember(
                member, Roles.Find(role => string.Equals(role.DescriptorName, groupRoleName, StringComparison.Ordinal))
            );

        public bool AddMember(Player member, GroupRole groupRole)
        {
            if (groupRole == null)
            {
                throw new ArgumentNullException(nameof(groupRole));
            }

            if (HasMember(member))
            {
                return false;
            }

            if (!Roles.Contains(groupRole))
            {
                return false;
            }

            var membership = new GroupMembership
            {
                Group = this,
                Member = member,
                Role
            }

            Memberships.Add(membership);
            return true;
        }

        public bool RemoveMember(Player member)
        {
            var membership = FindMembership(member);
            return membership != null && Memberships.Remove(membership);
        }

        public bool TryInvite(Player inviter, Player invitee, out GroupInvite groupInvite)
        {
            groupInvite = null;

            if (inviter == null)
            {
                throw new ArgumentNullException(nameof(inviter));
            }

            if (invitee == null)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            var inviterMembership = FindMembership(inviter);
            if (inviterMembership == null)
            {
                return false;
            }

            if (invitee.TryFindMembership(GroupTypeId, out var inviteeMembership))
            {
                return false;
            }

            groupInvite = new GroupInvite
            {
                GroupId = Id,
                GroupName = Name,
                FromId = inviter.Id,
                FromName = inviter.Name,
                Expires = Globals.Timing.Milliseconds + Options.RequestTimeout
            };

            invitee.GroupInvites.Add(groupInvite);

            return true;
        }

        public bool Promote(Player promoter, Player promotee, string targetRole = null)
        {
            if (promoter == null)
            {
                throw new ArgumentNullException(nameof(promoter));
            }

            if (promotee == null)
            {
                throw new ArgumentNullException(nameof(promotee));
            }

            var promoterMembership = FindMembership(promoter);
            if (promoterMembership == null)
            {
                return false;
            }

            var promoteeMembership = FindMembership(promotee);
            if (promoteeMembership == null)
            {
                return false;
            }

            if (promoterMembership.GroupId != promoteeMembership.GroupId)
            {
                return false;
            }

            if (promoteeRoleIndex < 1)
            {
            }
        }
    }

    public class GroupRole
    {
        public string DescriptorName { get; set; }

        public string Name { get; set; }
    }

    public class GroupMembership
    {
        [ForeignKey(nameof(Group))] public Guid GroupId { get; private set; }

        public virtual Group Group { get; private set; }

        [ForeignKey(nameof(Member))] public Guid MemberId { get; private set; }

        public virtual Player Member { get; private set; }

        #region Role Reference

        private string mRoleName;

        private GroupRole mRole;

        public string RoleName
        {
            get => mRoleName ?? mRole?.DescriptorName;
            set
            {
                mRole = Group?.FindRole(value);
                mRoleName = mRole?.DescriptorName ?? value;
            }
        }

        [NotMapped]
        public GroupRole Role
        {
            get => mRole ?? (mRole = Group.FindRole(mRoleName));
            set
            {
                mRole = value;
                mRoleName = mRole?.DescriptorName;
            }
        }

        [NotMapped] public GroupRoleDescriptor RoleDescriptor => Group?.FindRoleDescriptor(mRoleName);

        #endregion Role Reference

        #region Role Permissions

        public bool HasPermission(GroupPermissions groupPermissions) =>
            RoleDescriptor?.Permissions.HasFlag(groupPermissions) ?? false;

        #endregion
    }

    public class GroupInvite
    {
        public Guid GroupId { get; set; }

        public string GroupName { get; set; }

        public Guid FromId { get; set; }

        public string FromName { get; set; }

        public long Expires { get; set; }
    }
}
