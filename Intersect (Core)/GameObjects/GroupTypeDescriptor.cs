using Intersect.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Intersect.GameObjects
{
    public sealed class GroupTypeDescriptor : DatabaseObject<GroupTypeDescriptor>
    {
        private string mNameFormat = "^[-_. a-zA-Z0-9]{3,}$";

        private Regex mNameFormatRegex;

        public string NameFormat
        {
            get => mNameFormat;
            set
            {
                mNameFormat = value;
                mNameFormatRegex = null;
            }
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Regex NameFormatRegex => mNameFormatRegex = (mNameFormatRegex ?? new Regex(mNameFormat));

        public Color ChatColor { get; set; }

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

        [Column(nameof(Roles)), JsonIgnore]
        protected string SerializedRoles
        {
            get => JsonConvert.SerializeObject(Roles);
            set => Roles =
                JsonConvert.DeserializeObject<List<GroupRoleDescriptor>>(
                    string.IsNullOrWhiteSpace(value) ? "[]" : value
                );
        }

        [Column(nameof(MetaProperties)), JsonIgnore]
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
        public Guid Id { get; set; }

        public bool IsDefault { get; set; }

        public bool IsOwner { get; set; }

        public string Name { get; set; }

        public int MemberLimit { get; set; }

        public GroupPermissions Permissions { get; set; }

        public GroupRoleDescriptor() => Id = Guid.NewGuid();
    }

    // 
    // 
    // | Unallocated       | Permissions       | Group Admin       | Membership        |
    //  64        56        48        40        32        24        16        8       0
    //  0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000
    //  8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421 8421
    [Flags]
    public enum GroupPermissions
    {
        Leave = 0x0001,

        Invite = 0x0002,

        Kick = 0x0004,

        Promote = 0x0010,

        PromoteToOwnLevel = 0x0011,

        Disband = 0x0001 << 16,

        RenameGroup = 0x0002 << 16,

        CreateDeleteRoles = 0x0100 << 16,

        RenameRoles = 0x0200 << 16,

        SelectDefaultRole = 0x0400 << 16,

        ConfigureRolePermissions = 0x0001 << 32
    }
}
