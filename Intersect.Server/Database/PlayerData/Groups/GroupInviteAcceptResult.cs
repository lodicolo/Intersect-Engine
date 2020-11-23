namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupInviteAcceptResult
    {
        Success,

        GroupDoesNotExist,

        GroupFull,

        MemberHasConflictingGroup,

        MemberDeclined,

        MemberAccepted = Success
    }
}
