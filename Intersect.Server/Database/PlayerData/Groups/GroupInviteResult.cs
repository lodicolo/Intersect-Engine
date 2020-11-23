namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupInviteResult : int
    {
        Success,

        InviterNotInGroup,

        InviterMissingPermission,

        InviteeInGroup,

        GroupFull
    }
}
