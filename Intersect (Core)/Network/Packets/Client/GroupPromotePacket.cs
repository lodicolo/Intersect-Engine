using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupPromotePacket : GroupTargetPacket
    {
        public Guid? RoleId { get; set; }

        /// <inheritdoc />
        public GroupPromotePacket(Guid groupId, Guid targetId, string targetName, Guid? roleId) : base(
            groupId, targetId, targetName
        )
        {
            RoleId = roleId;
        }
    }
}
