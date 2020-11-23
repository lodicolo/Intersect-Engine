using Intersect.GameObjects;
using Intersect.Server.Entities;

using System;
using System.Collections.Generic;

using Intersect.Groups;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public interface IGroupLogic
    {
        #region Utility

        GroupRoleDescriptor FindRoleDescriptor(Group group, Guid roleId);

        Guid FindRoleId(Group group, Guid inviterRoleId, Guid? suggestedRoleId);

        GroupRoleDescriptor FindDefaultRoleDescriptor(Group group);

        Guid FindDefaultRoleId(Group group);

        GroupMemberAddResult AddMember(Guid groupId, Guid memberId, Guid roleId);

        GroupMemberAddResult AddMember(Guid groupId, Player member, Guid roleId);

        GroupMemberAddResult AddMember(Group group, Guid memberId, Guid roleId);

        GroupMemberAddResult AddMember(Group group, Player member, Guid roleId);

        GroupMemberRemoveResult RemoveMember(Guid groupId, Guid memberId);

        GroupMemberRemoveResult RemoveMember(Guid groupId, Player member);

        GroupMemberRemoveResult RemoveMember(Group group, Guid memberId);

        GroupMemberRemoveResult RemoveMember(Group group, Player member);

        GroupMemberRemoveResult RemoveMembership(GroupMembership groupMembership);

        bool TryAddMember(Group group, Player member, Guid roleId);

        bool TryRemoveMember(Group group, Player member);

        bool TryRemoveMembership(GroupMembership groupMembership);

        #endregion

        #region Creation

        GroupCreateResult Create(Guid groupTypeId, Player owner, string name);

        bool TryCreate(Guid groupTypeId, Player owner, string name);

        #endregion

        #region Disband

        GroupDisbandResult Disband(Guid groupId, Player disbander);

        GroupDisbandResult Disband(Group group, Player disbander);

        bool TryDisband(Guid groupId, Player disbander);

        bool TryDisband(Group group, Player disbander);

        #endregion

        #region Kick

        GroupKickResult Kick(Guid groupId, Player kicker, Player target);

        GroupKickResult Kick(Group group, Player kicker, Player target);

        GroupKickResult Kick(Group group, Player kicker, GroupMembership targetMembership);

        bool TryKick(Guid groupId, Player kicker, Player target);

        bool TryKick(Group group, Player kicker, Player target);

        bool TryKick(Group group, Player kicker, GroupMembership targetMembership);

        #endregion

        #region Leaving

        GroupLeaveResult Leave(Guid groupId, Player member);

        GroupLeaveResult Leave(Group group, Player member);

        bool TryLeave(Guid groupId, Player member);

        bool TryLeave(Group group, Player member);

        #endregion

        #region Experience

        bool TryDistributeExperience(Group group, Player source, long experience, Action<Player> additionalOperations = default);

        bool TryDistributeExperience(Group group, Player source, long experience, Func<Player, bool> additionalOperations = default);

        #endregion

        #region Invitation

        GroupInviteAcceptResult Accept(GroupInvite groupInvite, Player accepter, bool accept);

        GroupInviteResult Invite(Group group, Player inviter, Player invitee, Guid? suggestedRoleId = default);

        bool TryAccept(GroupInvite groupInvite, Player accepter, bool accept);

        bool TryInvite(Group group, Player inviter, Player invitee, Guid? suggestedRoleId = default);

        GroupInvite CreateInvite(Group group, Player inviter, Player invitee, Guid roleId);

        #endregion

        #region Promotion

        GroupPromoteResult Promote(Group group, Player promoter, Player promotee, Guid? promotionRoleId = default);
        #region Promotion

        GroupPromoteResult Promote(Group group, Player promoter, GroupMembership promoteeMembership, Guid? promotionRoleId = default);

        bool TryPromote(Group group, Player promoter, Player promotee, Guid? promotionRoleId = default);

        bool TryPromote(Group group, Player promoter, GroupMembership promoteeMembership, Guid? promotionRoleId = default);

        #endregion
    }
}
