using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupKickPacket : GroupTargetPacket
    {
        /// <inheritdoc />
        public GroupKickPacket(Guid groupId, Guid targetId, string targetName) : base(
            groupId, targetId, targetName
        )
        {
        }
    }
}
