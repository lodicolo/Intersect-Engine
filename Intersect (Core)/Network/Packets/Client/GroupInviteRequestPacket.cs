using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupInviteRequestPacket : GroupTargetPacket
    {
        public GroupInviteRequestPacket(Guid groupId, Guid targetId, string targetName) : base(
            groupId, targetId, targetName
        )
        {
        }
    }
}
