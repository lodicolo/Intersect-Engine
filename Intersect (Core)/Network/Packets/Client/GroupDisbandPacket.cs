using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupDisbandPacket : GroupPacket
    {
        /// <inheritdoc />
        public GroupDisbandPacket(Guid groupId) : base(groupId)
        {
        }
    }
}
