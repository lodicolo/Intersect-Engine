using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intersect.Models;

namespace Intersect.Membership
{
    public interface IGroupMember : INamedObject
    {
        bool IsInGroup(Guid groupId);

        bool IsInGroup(Group group);
    }
}
