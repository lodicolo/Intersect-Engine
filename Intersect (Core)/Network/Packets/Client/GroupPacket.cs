using System;

namespace Intersect.Network.Packets.Client
{
    public abstract class GroupPacket : CerasPacket
    {
        public Guid GroupId { get; set; }

        protected GroupPacket(Guid groupId)
        {
            GroupId = groupId;
        }
    }
}
