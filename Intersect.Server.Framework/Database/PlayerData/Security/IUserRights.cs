using System.Collections.Immutable;

namespace Intersect.Server.Framework.Database.PlayerData.Security
{
    public interface IUserRights
    {
        bool Api { get; set; }
        IApiRoles ApiRoles { get; set; }
        bool Ban { get; set; }
        bool Editor { get; set; }
        bool IsAdmin { get; }
        bool IsModerator { get; }
        bool Kick { get; set; }
        bool Mute { get; set; }
        ImmutableList<string> Roles { get; }

        bool Equals(object obj);
        int GetHashCode();
    }
}