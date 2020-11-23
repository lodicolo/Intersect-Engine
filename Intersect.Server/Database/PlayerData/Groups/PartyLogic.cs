using Intersect.GameObjects;
using Intersect.GameObjects.Events;
using Intersect.Server.Entities;

using System;
using System.Linq;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public class PartyLogic : GroupLogic
    {
        public const string NpcDeathCommonEventStartRange = "NpcDeathCommonEventStartRange";

        public override GroupPromoteResult Promote(
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

            if (!promoterMembership.HasPermission(GroupPermissions.Promote))
            {
                return GroupPromoteResult.PromoterInsufficientPermissions;
            }

            promoteeMembership.RoleId = promoterMembership.RoleId;
            promoterMembership.RoleId = FindDefaultRoleId(group);

            return GroupPromoteResult.Success;
        }

        public override GroupLeaveResult Leave(Group group, Player member)
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
                switch (Disband(group, member))
                {
                    case GroupDisbandResult.GroupDoesNotExist:
                        return GroupLeaveResult.GroupDoesNotExist;

                    case GroupDisbandResult.PlayerDoesNotExist:
                        return GroupLeaveResult.PlayerDoesNotExist;

                    case GroupDisbandResult.PlayerNotInGroup:
                        return GroupLeaveResult.PlayerNotInGroup;

                    case GroupDisbandResult.Success:
                        return GroupLeaveResult.Success;

                    default:
                        throw new Exception("Failed to disband party.");
                }
            }

            if (!group.TryFindMembership(member, out var membership))
            {
                return GroupLeaveResult.PlayerNotInGroup;
            }

            if (membership.RoleDescriptor.IsOwner)
            {
                var newLeader = group.Members.FirstOrDefault(groupMember => groupMember.Id != member.Id);
                if (TryPromote(group, member, newLeader, membership.RoleId))
                {
                    throw new Exception("Failed to promote another member.");
                }
            }

            if (!RemoveMember(membership))
            {
                throw new Exception($"Failed to remove {membership.MemberId} from {group.Id} ({group.GroupTypeId}).");
            }

            return GroupLeaveResult.Success;
        }

        public bool TryDispatchPartyEvent(Group group, Player source, EventBase eventDescriptor)
        {
            if (group == default)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (source == default)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (eventDescriptor == default)
            {
                throw new ArgumentNullException(nameof(eventDescriptor));
            }

            if (!group.GroupType.MetaProperties.TryGetValue(NpcDeathCommonEventStartRange, out var objectRange))
            {
                return false;
            }

            if (!(objectRange is int intRange))
            {
                return false;
            }

            var membersInEventRange =
                group.Members.Where(groupMember => groupMember.InRangeOf(source, intRange)).ToList();

            foreach (var groupMember in membersInEventRange)
            {
                groupMember.StartCommonEvent(eventDescriptor);
            }

            return true;
        }
    }
}
