using Intersect.Models;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Intersect.Membership
{
    public class Group : INamedObject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; private set; }

        [NotNull] public string Name { get; set; }

        /// <summary>
        /// If this group is a transient (non-persisted) group. Parties are an example of transient groups.
        /// </summary>
        [NotMapped]
        public bool IsTransient { get; set; }

        [NotNull] public virtual List<GroupMembership> Memberships { get; private set; } = new List<GroupMembership>();

        [NotNull, NotMapped]
        public List<IGroupMember> Members => Memberships.Select(membership => membership.Member).ToList();

        [NotNull] public virtual List<GroupRole> Roles { get; private set; } = new List<GroupRole>();

        #region Lookup

        public IGroupMember FindMember(Guid id) => Members.FirstOrDefault(member => member.Id == id);

        public IGroupMember FindMember(string name) =>
            Members.FirstOrDefault(member => string.Equals(member.Name, name, StringComparison.Ordinal));

        public TGroupMember FindMember<TGroupMember>(Guid id) where TGroupMember : class, IGroupMember =>
            Members.FirstOrDefault(member => member.Id == id) as TGroupMember;

        public TGroupMember FindMember<TGroupMember>(string name) where TGroupMember : class, IGroupMember =>
            Members.FirstOrDefault(
                member => string.Equals(member.Name, name, StringComparison.Ordinal)
            ) as TGroupMember;

        public IEnumerable<IGroupMember> FindMembers(GroupRole role) => Members.Where(member => member.HasRole(role));

        public IEnumerable<TGroupMember> FindMembers<TGroupMember>(GroupRole role) where TGroupMember : class, IGroupMember => Members.Where(member => member.HasRole(role)).Cast<TGroupMember>();

        public IEnumerable<IGroupMember> FindMembers(GroupPermission permission) => Members.Where(member => member.HasPermission(permission));

        public IEnumerable<TGroupMember> FindMembers<TGroupMember>(GroupPermission permission) where TGroupMember : class, IGroupMember => Members.Where(member => member.HasPermission(permission)).Cast<TGroupMember>(); 
        
        #endregion Lookup
    }
}
