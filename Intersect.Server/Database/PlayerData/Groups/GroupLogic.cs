using Intersect.GameObjects;
using Intersect.Groups;
using Intersect.Server.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public class GroupLogic : IGroupLogic
    {
        #region Static Utility

        private static Dictionary<GroupSpecialType, IGroupLogic> GroupLogicRegistry { get; } =
            new Dictionary<GroupSpecialType, IGroupLogic>
            {
                {GroupSpecialType.None, new GroupLogic()},
                {GroupSpecialType.Party, new PartyLogic()}
            };

        public static IGroupLogic GetLogic(GroupSpecialType groupSpecialType) =>
            GroupLogicRegistry.TryGetValue(groupSpecialType, out var groupLogic)
                ? groupLogic
                : GroupLogicRegistry[GroupSpecialType.None];

        public static IGroupLogic GetLogic(Group group) => GetLogic(
            group?.GroupType.SpecialType ?? throw new ArgumentNullException(nameof(group))
        );

        #endregion

        #region Utility

        /// <inheritdoc />
        public virtual GroupRoleDescriptor FindRoleDescriptor(Group group, Guid roleId)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            return group.RoleDescriptors.FirstOrDefault(role => role.Id == roleId);
        }

        /// <inheritdoc />
        public virtual Guid FindRoleId(Group group, Guid inviterRoleId, Guid? suggestedRoleId)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var foundRoleId = FindDefaultRoleId(group);

            if (suggestedRoleId != default)
            {
                var foundRole = group.RoleDescriptors.SkipWhile(role => role.Id != inviterRoleId)
                    .FirstOrDefault(role => role.Id == suggestedRoleId);

                foundRoleId = foundRole?.Id ?? foundRoleId;
            }

            return foundRoleId;
        }

        /// <inheritdoc />
        public virtual GroupRoleDescriptor FindDefaultRoleDescriptor(Group group)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var groupType = group.GroupType;
            var roleDescriptor = groupType.Roles.FirstOrDefault(role => role.IsDefault);
            if (roleDescriptor == default)
            {
                throw new MissingDataException(
                    $"Unable to find default role for group '{groupType.Name}' ({groupType.Id})."
                );
            }

            return roleDescriptor;
        }

        /// <inheritdoc />
        public virtual Guid FindDefaultRoleId(Group group) => FindDefaultRoleDescriptor(group).Id;

        /// <inheritdoc />
        public virtual GroupMemberAddResult AddMember(Guid groupId, Guid memberId, Guid roleId)
        {
            if (!Group.TryFindGroup(groupId, out var group))
            {
                return GroupMemberAddResult.GroupDoesNotExist;
            }

            return AddMember(group, memberId, roleId);
        }

        /// <inheritdoc />
        public virtual GroupMemberAddResult AddMember(Guid groupId, Player member, Guid roleId)
        {
            if (!Group.TryFindGroup(groupId, out var group))
            {
                return GroupMemberAddResult.GroupDoesNotExist;
            }

            return AddMember(group, member, roleId);
        }

        /// <inheritdoc />
        public virtual GroupMemberAddResult AddMember(Group group, Guid memberId, Guid roleId)
        {
            var member = Player.Find(memberId);
            if (member == default)
            {
                return GroupMemberAddResult.PlayerDoesNotExist;
            }

            return AddMember(group, member, roleId);
        }

        /// <inheritdoc />
        public virtual GroupMemberAddResult AddMember(Group group, Player member, Guid roleId)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (member == default)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (member.TryFindMembership(group.GroupTypeId, out var _))
            {
                return GroupMemberAddResult.PlayerIsAlreadyMember;
            }

            var roleDescriptor = FindRoleDescriptor(group, roleId);
            if (roleDescriptor == default)
            {
                return GroupMemberAddResult.RoleInvalid;
            }

            group.Memberships.Add(
                new GroupMembership(group, member)
                {
                    RoleId = roleId
                }
            );

            return GroupMemberAddResult.Success;
        }

        /// <inheritdoc />
        public virtual GroupMemberRemoveResult RemoveMember(Guid groupId, Guid memberId)
        {
            if (!Group.TryFindGroup(groupId, out var group))
            {
                return GroupMemberRemoveResult.GroupDoesNotExist;
            }

            return RemoveMember(group, memberId);
        }

        /// <inheritdoc />
        public virtual GroupMemberRemoveResult RemoveMember(Guid groupId, Player member)
        {
            if (!Group.TryFindGroup(groupId, out var group))
            {
                return GroupMemberRemoveResult.GroupDoesNotExist;
            }

            return RemoveMember(group, member);
        }

        /// <inheritdoc />
        public virtual GroupMemberRemoveResult RemoveMember(Group group, Guid memberId)
        {
            var member = Player.Find(memberId);
            if (member == default)
            {
                return GroupMemberRemoveResult.PlayerDoesNotExist;
            }

            return RemoveMember(group, member);
        }

        /// <inheritdoc />
        public virtual GroupMemberRemoveResult RemoveMember(Group group, Player member)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (member == default)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (!group.TryFindMembership(member, out var membership))
            {
                return GroupMemberRemoveResult.PlayerIsNotMember;
            }

            return RemoveMembership(membership);
        }

        /// <inheritdoc />
        public GroupMemberRemoveResult RemoveMembership(GroupMembership groupMembership)
        {
            if (groupMembership == default)
            {
                throw new ArgumentNullException(nameof(groupMembership));
            }

            groupMembership.Group?.Memberships.Remove(groupMembership);
            groupMembership.Member?.Memberships.Remove(groupMembership);

            using (var playerContext = DbInterface.PlayerContext)
            {
                playerContext.GroupMemberships.Remove(groupMembership);
            }

            return GroupMemberRemoveResult.Success;
        }

        /// <inheritdoc />
        public bool TryAddMember(Group group, Player member, Guid roleId) =>
            AddMember(group, member, roleId) == GroupMemberAddResult.Success;

        /// <inheritdoc />
        public bool TryRemoveMember(Group group, Player member) =>
            RemoveMember(group, member) == GroupMemberRemoveResult.Success;

        /// <inheritdoc />
        public bool TryRemoveMembership(GroupMembership groupMembership) =>
            RemoveMembership(groupMembership) == GroupMemberRemoveResult.Success;

        #endregion

        #region Creation

        public virtual GroupCreateResult Create(Guid groupTypeId, Player owner, string name)
        {
            if (owner == default)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            var groupTypeDescriptor = GroupTypeDescriptor.Get(groupTypeId);
            if (groupTypeDescriptor == default)
            {
                throw new ArgumentException($"Invalid group type ID: {groupTypeId}", nameof(groupTypeId));
            }

            if (!groupTypeDescriptor.NameFormatRegex.IsMatch(name))
            {
                return GroupCreateResult.GroupNameInvalid;
            }

            if (owner.TryFindMembership(groupTypeId, out _))
            {
                return GroupCreateResult.PlayerAlreadyInGroup;
            }

            var group = new Group(name, groupTypeId, owner);
            using (var playerContext = DbInterface.PlayerContext)
            {
                playerContext.Groups.Add(group);
            }

            return GroupCreateResult.Success;
        }

        public bool TryCreate(Guid groupTypeId, Player owner, string name) =>
            Create(groupTypeId, owner, name) == GroupCreateResult.Success;

        #endregion

        #region Disband

        public virtual GroupDisbandResult Disband(Guid groupId, Player disbander)
        {
            if (Group.TryFindGroup(groupId, out var group))
            {
                return Disband(group, disbander);
            }

            return GroupDisbandResult.GroupDoesNotExist;
        }

        public virtual GroupDisbandResult Disband(Group group, Player disbander)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (disbander == default)
            {
                throw new ArgumentNullException(nameof(disbander));
            }

            if (!group.TryFindMembership(disbander, out var membership))
            {
                return GroupDisbandResult.PlayerNotInGroup;
            }

            if (!membership.HasPermission(GroupPermissions.Disband))
            {
                return GroupDisbandResult.PlayerInsufficientPermissions;
            }

            TryRemoveMember(group, disbander);

            using (var playerContext = DbInterface.PlayerContext)
            {
                playerContext.Groups.Remove(group);
            }

            return GroupDisbandResult.Success;
        }

        public bool TryDisband(Guid groupId, Player disbander) =>
            Disband(groupId, disbander) == GroupDisbandResult.Success;

        public bool TryDisband(Group group, Player disbander) =>
            Disband(group, disbander) == GroupDisbandResult.Success;

        #endregion

        #region Kick

        protected virtual GroupKickResult Kick(
            Group group,
            GroupMembership kickerMembership,
            GroupMembership targetMembership
        )
        {
            if (!kickerMembership.HasPermission(GroupPermissions.Kick))
            {
                return GroupKickResult.PlayerInsufficientPermissions;
            }

            if (TryRemoveMembership(targetMembership))
            {
                return GroupKickResult.Success;
            }

            throw new Exception($"Failed to remove {targetMembership.MemberId} from {targetMembership.GroupId}.");
        }

        public virtual GroupKickResult Kick(Guid groupId, Player kicker, Player target)
        {
            if (Group.TryFindGroup(groupId, out var group))
            {
                return Kick(group, kicker, target);
            }

            return GroupKickResult.GroupDoesNotExist;
        }

        public virtual GroupKickResult Kick(Group group, Player kicker, Player target)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (kicker == null)
            {
                throw new ArgumentNullException(nameof(kicker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!group.TryFindMembership(kicker, out var kickerMembership))
            {
                return GroupKickResult.KickerNotInGroup;
            }

            if (!group.TryFindMembership(target, out var targetMembership))
            {
                return GroupKickResult.TargetNotInGroup;
            }

            return Kick(group, kickerMembership, targetMembership);
        }

        /// <inheritdoc />
        public GroupKickResult Kick(Group @group, Player kicker, GroupMembership targetMembership)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (kicker == null)
            {
                throw new ArgumentNullException(nameof(kicker));
            }

            if (targetMembership == null)
            {
                throw new ArgumentNullException(nameof(targetMembership));
            }

            if (!group.TryFindMembership(kicker, out var kickerMembership))
            {
                return GroupKickResult.KickerNotInGroup;
            }

            return Kick(group, kickerMembership, targetMembership);
        }

        public virtual bool TryKick(Guid groupId, Player kicker, Player target) =>
            Kick(groupId, kicker, target) == GroupKickResult.Success;

        public virtual bool TryKick(Group group, Player kicker, Player target) =>
            Kick(group, kicker, target) == GroupKickResult.Success;

        /// <inheritdoc />
        public bool TryKick(Group @group, Player kicker, GroupMembership targetMembership) =>
            Kick(group, kicker, targetMembership) == GroupKickResult.Success;

        #endregion

        #region Leaving

        public virtual GroupLeaveResult Leave(Guid groupId, Player member)
        {
            if (Group.TryFindGroup(groupId, out var group))
            {
                return Leave(group, member);
            }

            return GroupLeaveResult.GroupDoesNotExist;
        }

        public virtual GroupLeaveResult Leave(Group group, Player member)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (member == default)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (group.Memberships.Count == 1)
            {
                return GroupLeaveResult.PlayerIsLastMember;
            }

            switch (RemoveMember(group, member))
            {
                case GroupMemberRemoveResult.PlayerIsNotMember:
                    return GroupLeaveResult.PlayerNotInGroup;

                case GroupMemberRemoveResult.Success:
                    return GroupLeaveResult.Success;

                default:
                    throw new Exception($"Failed to remove member {member.Id} from {group.Id} ({group.GroupTypeId}).");
            }
        }

        public bool TryLeave(Guid groupId, Player member) => Leave(groupId, member) == GroupLeaveResult.Success;

        public bool TryLeave(Group group, Player member) => Leave(group, member) == GroupLeaveResult.Success;

        #endregion

        #region Experience

        public virtual bool TryDistributeExperience(
            Group group,
            Player source,
            long experience,
            Action<Player> additionalOperations = default
        ) => TryDistributeExperience(
            group, source, experience, additionalOperations == default
                ? default(Func<Player, bool>)
                : player =>
                {
                    additionalOperations(player);
                    return true;
                }
        );

        public virtual bool TryDistributeExperience(
            Group group,
            Player source,
            long experience,
            Func<Player, bool> additionalOperations = default
        )
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (source == default)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var membersInExperienceRange = group.Members.Where(
                    groupMember => groupMember.InRangeOf(source, group.GroupType.SharedExperienceRange)
                )
                .ToList();

            var partialExperience = experience / membersInExperienceRange.Count;
            var success = true;
            foreach (var groupMember in membersInExperienceRange)
            {
                groupMember.GiveExperience(partialExperience);
                success |= additionalOperations?.Invoke(groupMember) ?? true;
            }

            return success;
        }

        #endregion

        #region Invitation

        /// <inheritdoc />
        public virtual GroupInviteAcceptResult Accept(GroupInvite groupInvite, Player accepter, bool accept)
        {
            if (groupInvite == default)
            {
                throw new ArgumentNullException(nameof(groupInvite));
            }

            if (accepter == default)
            {
                throw new ArgumentNullException(nameof(accepter));
            }

            if (!Group.TryFindGroup(groupInvite.GroupId, out var group))
            {
                return GroupInviteAcceptResult.GroupDoesNotExist;
            }

            if (group.IsFull)
            {
                return GroupInviteAcceptResult.GroupFull;
            }

            if (!accept)
            {
                accepter.GroupInvites.Remove(groupInvite);
                return GroupInviteAcceptResult.MemberDeclined;
            }

            if (accepter.TryFindMembership(group.GroupTypeId, out _))
            {
                return GroupInviteAcceptResult.MemberHasConflictingGroup;
            }

            var roleId = FindRoleId(group, groupInvite.FromId, groupInvite.RoleId);
            if (TryAddMember(group, accepter, roleId))
            {
                return GroupInviteAcceptResult.MemberAccepted;
            }

            throw new Exception($"Failed to add {accepter.Id} to {group.Id} ({group.GroupTypeId}).");
        }

        /// <inheritdoc />
        public virtual GroupInviteResult Invite(
            Group group,
            Player inviter,
            Player invitee,
            Guid? suggestedRoleId = default
        )
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (inviter == default)
            {
                throw new ArgumentNullException(nameof(inviter));
            }

            if (invitee == default)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            if (group.IsFull)
            {
                return GroupInviteResult.GroupFull;
            }

            if (!group.TryFindMembership(inviter, out var inviterMembership))
            {
                return GroupInviteResult.InviterNotInGroup;
            }

            if (!inviterMembership.HasPermission(GroupPermissions.Invite))
            {
                return GroupInviteResult.InviterMissingPermission;
            }

            if (invitee.TryFindMembership(group.GroupTypeId, out _))
            {
                return GroupInviteResult.InviteeInGroup;
            }

            var invitationRoleId = FindRoleId(group, inviterMembership.Role.DescriptorId, suggestedRoleId);
            var invite = CreateInvite(group, inviter, invitee, invitationRoleId);
            invitee.GroupInvites.Add(invite);
            return GroupInviteResult.Success;
        }

        /// <inheritdoc />
        public bool TryAccept(GroupInvite groupInvite, Player accepter, bool accept) =>
            Accept(groupInvite, accepter, accept) == GroupInviteAcceptResult.Success;

        /// <inheritdoc />
        public bool TryInvite(Group group, Player inviter, Player invitee, Guid? suggestedRoleId = default) =>
            Invite(group, inviter, invitee, suggestedRoleId) == GroupInviteResult.Success;

        /// <inheritdoc />
        public virtual GroupInvite CreateInvite(Group group, Player inviter, Player invitee, Guid roleId)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (inviter == default)
            {
                throw new ArgumentNullException(nameof(inviter));
            }

            if (invitee == default)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            return new GroupInvite
            {
                GroupId = group.Id,
                GroupName = group.Name,
                FromId = inviter.Id,
                FromName = inviter.Name,
                RoleId = roleId
            };
        }

        #endregion

        #region Promotion

        protected virtual GroupPromoteResult Promote(
            Group group,
            GroupMembership promoterMembership,
            GroupMembership promoteeMembership,
            Guid? promotionRoleId
        )
        {
            if (!promoterMembership.HasPermission(GroupPermissions.Promote))
            {
                return GroupPromoteResult.PromoterInsufficientPermissions;
            }

            var promoterRoleIndex = group.FindRoleDescriptorIndex(promoterMembership.RoleId);
            var promoteeRoleIndex = group.FindRoleDescriptorIndex(promoteeMembership.RoleId);
            var promotionRoleIndex = promotionRoleId.HasValue
                ? group.FindRoleDescriptorIndex(promotionRoleId.Value)
                : promoteeRoleIndex - 1;

            var targetRoleIndex = Math.Max(0, promotionRoleIndex);
            if (targetRoleIndex < promoterRoleIndex)
            {
                return GroupPromoteResult.PromoterInsufficientPermissions;
            }

            if (targetRoleIndex == promoterRoleIndex)
            {
                if (!promoterMembership.HasPermission(GroupPermissions.PromoteToOwnLevel))
                {
                    return GroupPromoteResult.PromoterInsufficientPermissions;
                }
            }

            var promotionRoleDescriptor = group.RoleDescriptors[promotionRoleIndex];
            var membersWithRole = group.MembersWithRole(promotionRoleDescriptor.Id);
            if (promotionRoleDescriptor.MemberLimit > 0 && membersWithRole >= promotionRoleDescriptor.MemberLimit)
            {
                return GroupPromoteResult.PromotionRoleTooManyMembers;
            }

            promoteeMembership.RoleId = promotionRoleDescriptor.Id;
            return GroupPromoteResult.Success;
        }

        /// <inheritdoc />
        public virtual GroupPromoteResult Promote(
            Group group,
            Player promoter,
            Player promotee,
            Guid? promotionRoleId = default
        )
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (promoter == default)
            {
                throw new ArgumentNullException(nameof(promoter));
            }

            if (promotee == default)
            {
                throw new ArgumentNullException(nameof(promotee));
            }

            if (!group.TryFindMembership(promoter, out var promoterMembership))
            {
                return GroupPromoteResult.PromoterNotInGroup;
            }

            if (!group.TryFindMembership(promotee, out var promoteeMembership))
            {
                return GroupPromoteResult.PromoteeNotInGroup;
            }

            return Promote(group, promoterMembership, promoteeMembership, promotionRoleId);
        }

        /// <inheritdoc />
        public GroupPromoteResult Promote(
            Group @group,
            Player promoter,
            GroupMembership promoteeMembership,
            Guid? promotionRoleId = default
        )
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (promoter == default)
            {
                throw new ArgumentNullException(nameof(promoter));
            }

            if (promoteeMembership == default)
            {
                throw new ArgumentNullException(nameof(promoteeMembership));
            }

            if (!group.TryFindMembership(promoter, out var promoterMembership))
            {
                return GroupPromoteResult.PromoterNotInGroup;
            }

            return Promote(group, promoterMembership, promoteeMembership, promotionRoleId);
        }

        /// <inheritdoc />
        public bool TryPromote(Group group, Player promoter, Player promotee, Guid? promotionRoleId = default) =>
            Promote(group, promoter, promotee, promotionRoleId) == GroupPromoteResult.Success;

        /// <inheritdoc />
        public bool TryPromote(
            Group @group,
            Player promoter,
            GroupMembership promoteeMembership,
            Guid? promotionRoleId = default
        ) =>
            Promote(group, promoter, promoteeMembership, promotionRoleId) == GroupPromoteResult.Success;

        #endregion
    }
}
