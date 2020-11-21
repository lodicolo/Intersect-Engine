using System;

using Intersect.Models;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Intersect.Config;

namespace Intersect.GameObjects
{
    public sealed class GroupTypeDescriptor : DatabaseObject<GroupTypeDescriptor>
    {
        public int MemberLimit { get; set; }

        public bool Persistent { get; set; }

        public int InviteRange { get; set; }

        public int SharedExperienceRange { get; set; }

        public GroupSpecialType SpecialType { get; set; }

        [NotMapped] public List<GroupRoleDescriptor> Roles { get; set; } = new List<GroupRoleDescriptor>();

        [NotMapped] public Dictionary<string, object> MetaProperties { get; set; } = new Dictionary<string, object>();

        public TValue Get<TValue>(string propertyName, TValue defaultPropertyValue = default) =>
            MetaProperties.TryGetValue(propertyName, out var propertyValue)
                ? (TValue) propertyValue
                : defaultPropertyValue;

        public void Set(string propertyName, object propertyValue) => MetaProperties[propertyName] = propertyValue;

        [Column(nameof(Roles))]
        protected string SerializedRoles
        {
            get => JsonConvert.SerializeObject(Roles);
            set => Roles =
                JsonConvert.DeserializeObject<List<GroupRoleDescriptor>>(
                    string.IsNullOrWhiteSpace(value) ? "[]" : value
                );
        }

        [Column(nameof(MetaProperties))]
        protected string SerializedMetaProperties
        {
            get => JsonConvert.SerializeObject(MetaProperties);
            set => MetaProperties =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    string.IsNullOrWhiteSpace(value) ? "{}" : value
                );
        }

        public static GroupTypeDescriptor CreatePartyDescriptor() =>
            new GroupTypeDescriptor
            {
                Name = "Party",
                SpecialType = GroupSpecialType.Party,
                MemberLimit = 4,
                Persistent = true,
                InviteRange = 0,
                SharedExperienceRange = 0,
                MetaProperties =
                {
                    {"NpcDeathCommonEventStartRange", 0}
                },
                Roles =
                {
                    new GroupRoleDescriptor
                    {
                        Name = "Party Leader",
                        MemberLimit = 1,
                        Permissions = GroupPermissions.Disband |
                                      GroupPermissions.Invite |
                                      GroupPermissions.Kick |
                                      GroupPermissions.Promote |
                                      GroupPermissions.Leave
                    },
                    new GroupRoleDescriptor
                    {
                        Name = "Party Member",
                        Permissions = GroupPermissions.Leave
                    }
                }
            };
    }

    public enum GroupSpecialType
    {
        None,

        Party,

        Guild
    }

    public sealed class GroupRoleDescriptor
    {
        public string Name { get; set; }

        public int MemberLimit { get; set; }

        public bool UserCustomizable { get; set; }

        public GroupPermissions Permissions { get; set; }
    }

    [Flags]
    public enum GroupPermissions
    {
        Disband = 0x01,

        Invite = 0x02,

        Kick = 0x04,

        Promote = 0x08,

        Leave = 0x10
    }
}
