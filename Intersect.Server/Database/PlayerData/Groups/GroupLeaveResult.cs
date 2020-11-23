namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupLeaveResult
    {
        Success,

        GroupDoesNotExist,

        PlayerDoesNotExist,

        PlayerNotInGroup,

        PlayerIsLastMember
    }
}
