namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupMemberAddResult
    {
        Success,

        GroupDoesNotExist,

        PlayerDoesNotExist,

        PlayerIsAlreadyMember,

        RoleInvalid
    }
}
