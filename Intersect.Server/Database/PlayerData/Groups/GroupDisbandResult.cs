namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupDisbandResult
    {
        Success,

        GroupDoesNotExist,

        PlayerDoesNotExist,

        PlayerNotInGroup,

        PlayerInsufficientPermissions
    }
}
