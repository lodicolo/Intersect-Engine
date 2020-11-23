using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupLeavePacket : GroupPacket
    {
        /// <inheritdoc />
        public GroupLeavePacket(Guid groupId) : base(groupId)
        {
        }
    }
}
