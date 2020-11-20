using Intersect.GameObjects;
using Intersect.Server.Entities;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Intersect.Server.Database.PlayerData
{
    public class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public Guid Id { get; private set; }

        public Guid GroupTypeId { get; set; }

        [JsonIgnore, NotMapped]
        public GroupTypeDescriptor GroupType => GroupTypeDescriptor.Get(GroupTypeId);

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Deleted { get; set; }

        [JsonIgnore]
        public virtual List<Player> Members => Memberships.Select(membership => membership.Member).ToList();

        [JsonIgnore] public virtual List<GroupMembership> Memberships { get; set; } = new List<GroupMembership>();

        private GroupMembership FindMembership(Player member) =>
            Memberships.FirstOrDefault(membership => membership.MemberId == member.Id);

        public bool HasMember(Player member) => FindMembership(member) != null;

        public bool AddMember(Player member)
        {
            if (HasMember(member))
            {
                return false;
            }

            var membership = new GroupMembership(this, member);
            Memberships.Add(membership);
            return true;
        }

        public bool RemoveMember(Player member)
        {
            var membership = FindMembership(member);
            return membership != null && Memberships.Remove(membership);
        }
    }

    public class GroupMembership
    {
        [ForeignKey(nameof(Group))] public Guid GroupId { get; private set; }

        public virtual Group Group { get; private set; }

        [ForeignKey(nameof(Member))] public Guid MemberId { get; private set; }

        public virtual Player Member { get; private set; }

        protected GroupMembership() { }

        public GroupMembership(Group group, Player member)
        {
            Group = group;
            Member = member;
        }
    }
}
