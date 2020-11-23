using System;

namespace Intersect.Network.Packets.Client
{
    public abstract class GroupTargetPacket : GroupPacket
    {
        public Guid TargetId { get; set; }

        public string TargetName { get; set; }

        protected GroupTargetPacket(Guid groupId, Guid targetId, string targetName) : base(groupId)
        {
            TargetName = targetName;
        }
    }
}
