using System;

namespace Intersect.Network.Packets.Client
{
    public class GroupInviteResponsePacket : CerasPacket
    {
        public Guid InviteId { get; set; }

        public bool Accept { get; set; }

        public GroupInviteResponsePacket(Guid inviteId, bool accept)
        {
            InviteId = inviteId;
            Accept = accept;
        }
    }
}
