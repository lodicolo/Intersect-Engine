using Intersect.GameObjects;
using Intersect.Server.Entities;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public sealed partial class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public Guid Id { get; private set; }

        public Guid GroupTypeId { get; set; }

        [JsonIgnore, NotMapped] public GroupTypeDescriptor GroupType => GroupTypeDescriptor.Get(GroupTypeId);

        [JsonIgnore, NotMapped] public string GroupTypeName => GroupType.Name;

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Deleted { get; set; }

        public int MemberLimit { get; set; }

        [NotMapped] public bool IsFull => Memberships.Count >= MemberLimit;

        [NotMapped] public List<GroupRole> Roles { get; }

        [NotMapped] public List<GroupRoleDescriptor> RoleDescriptors => GroupType?.Roles;

        [NotMapped] public IGroupLogic Logic => GroupLogic.GetLogic(this);

        [JsonIgnore]
        public List<Player> Members => Memberships.Select(membership => membership.Member).ToList();

        [JsonIgnore] public List<GroupMembership> Memberships { get; }

        private GroupMembership FindMembership(Guid memberId) =>
            Memberships.FirstOrDefault(membership => membership.MemberId == memberId);

        public bool TryFindMembership(Player member, out GroupMembership groupMembership)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            groupMembership = FindMembership(member.Id);
            return groupMembership != null;
        }

        public bool TryFindMembership(Guid memberId, out GroupMembership groupMembership)
        {
            groupMembership = FindMembership(memberId);
            return groupMembership != null;
        }

        public GroupRole FindRole(Guid roleId) =>
            Roles.Find(role => roleId == role.DescriptorId);

        public GroupRoleDescriptor FindRoleDescriptor(Guid roleId) =>
            RoleDescriptors?.Find(roleDescriptor => roleId == roleDescriptor.Id);

        public int FindRoleIndex(Guid roleId) =>
            Roles.FindIndex(role => roleId == role.DescriptorId);

        public int FindRoleDescriptorIndex(Guid roleId) =>
            RoleDescriptors?.FindIndex(roleDescriptor => roleId == roleDescriptor.Id) ?? -1;

        public int MembersWithRole(Guid roleId) => Memberships.Count(membership => membership.RoleId == roleId);

        public bool IsMember(Player member) => TryFindMembership(member, out _);

        public Group()
        {
            Roles = new List<GroupRole>();
            Memberships = new List<GroupMembership>();
        }

        public Group(string name, Guid groupTypeId, Player owner) : this(
            name, GroupTypeDescriptor.Get(groupTypeId), owner
        )
        {
        }

        public Group(string name, GroupTypeDescriptor groupTypeDescriptor, Player owner)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (groupTypeDescriptor == null)
            {
                throw new ArgumentNullException(nameof(groupTypeDescriptor));
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Name = name;
            GroupTypeId = groupTypeDescriptor.Id;
            Created = DateTime.UtcNow;
            MemberLimit = groupTypeDescriptor.MemberLimit;

            Roles = groupTypeDescriptor.Roles.Select(
                    roleDescriptor => new GroupRole
                    {
                        DescriptorId = roleDescriptor.Id,
                        Name = roleDescriptor.Name
                    }
                )
                .ToList();

            Memberships = new List<GroupMembership>
            {
                new GroupMembership(this, owner)
                {
                    RoleId = groupTypeDescriptor.Roles.Find(groupRoleDescriptor => groupRoleDescriptor.IsOwner).Id
                }
            };
        }
    }

    public class GroupRole
    {
        public Guid DescriptorId { get; set; }

        public string Name { get; set; }
    }

    public class GroupMembership
    {
        [ForeignKey(nameof(Group))] public Guid GroupId { get; private set; }

        public virtual Group Group { get; private set; }

        [ForeignKey(nameof(Member))] public Guid MemberId { get; private set; }

        public virtual Player Member { get; private set; }

        #region Role Reference

        private Guid mRoleId;

        private GroupRole mRole;

        public Guid RoleId
        {
            get => mRole?.DescriptorId ?? mRoleId;
            set
            {
                mRole = Group?.FindRole(value);
                mRoleId = mRole?.DescriptorId ?? value;
            }
        }

        [NotMapped] public string RoleName => Role?.Name;

        [NotMapped]
        public GroupRole Role
        {
            get => mRole ?? (mRole = Group.FindRole(mRoleId));
            set
            {
                mRole = value;
                mRoleId = mRole?.DescriptorId ?? Guid.Empty;
            }
        }

        [NotMapped] public GroupRoleDescriptor RoleDescriptor => Group?.FindRoleDescriptor(mRoleId);

        #endregion Role Reference

        #region Role Permissions

        public bool HasPermission(GroupPermissions groupPermissions) =>
            RoleDescriptor?.Permissions.HasFlag(groupPermissions) ?? false;

        #endregion

        public GroupMembership()
        {
        }

        public GroupMembership(Group group, Player member)
        {
            Group = group;
            Member = member;
        }
    }
}
