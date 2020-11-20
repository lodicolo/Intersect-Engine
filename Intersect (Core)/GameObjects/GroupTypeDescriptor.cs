using Intersect.Models;

namespace Intersect.GameObjects
{
    public class GroupTypeDescriptor : DatabaseObject<GroupTypeDescriptor>
    {
        public int MemberLimit { get; set; }
    }
}
