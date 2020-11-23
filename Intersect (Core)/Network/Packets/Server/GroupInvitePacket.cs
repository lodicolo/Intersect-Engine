using Intersect.Groups;

namespace Intersect.Network.Packets.Server
{
    public class GroupInvitePacket : CerasPacket
    {
        public GroupInvite GroupInvite { get; set; }

        public GroupInvitePacket(GroupInvite groupInvite)
        {
            GroupInvite = groupInvite;
        }
    }
}
