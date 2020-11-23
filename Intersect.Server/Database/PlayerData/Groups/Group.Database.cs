using System;
using System.Linq;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public sealed partial class Group
    {
        public static Group FindGroup(Guid groupId)
        {
            using (var playerContext = DbInterface.PlayerContext)
            {
                return playerContext.Groups.FirstOrDefault(group => group.Id == groupId);
            }
        }

        public static bool TryFindGroup(Guid groupId, out Group group) => (group = FindGroup(groupId)) != default;
    }
}
