namespace Intersect.Server.Database.PlayerData.Groups
{
    public enum GroupPromoteResult
    {
        Success,

        PromoterNotInGroup,

        PromoteeNotInGroup,

        PromoterInsufficientPermissions,

        PromotionRoleTooManyMembers
    }
}
