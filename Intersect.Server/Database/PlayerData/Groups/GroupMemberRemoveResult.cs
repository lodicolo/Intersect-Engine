namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupMemberRemoveResult
    {
        Success,

        GroupDoesNotExist,

        PlayerDoesNotExist,

        PlayerIsNotMember
    }
}
