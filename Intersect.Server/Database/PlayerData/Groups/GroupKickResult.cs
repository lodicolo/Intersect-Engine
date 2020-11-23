namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupKickResult
    {
        Success,

        GroupDoesNotExist,

        PlayerDoesNotExist,

        KickerNotInGroup,

        TargetNotInGroup,

        PlayerInsufficientPermissions
    }
}
