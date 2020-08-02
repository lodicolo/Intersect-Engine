using System;

using JetBrains.Annotations;

namespace Intersect.Membership
{
    /// <summary>
    /// 
    /// </summary>
    public struct GroupPermission : IEquatable<string>, IEquatable<GroupPermission>
    {
        /// <summary>
        /// Built-in permission that grants access to do anything to a group.
        /// </summary>
        public static readonly GroupPermission Owner = nameof(Owner);

        /// <summary>
        /// Built-in permission that grants access to update (modify) the group.
        /// </summary>
        public static readonly GroupPermission Update = nameof(Update);

        /// <summary>
        /// Built-in permission that grants access to delete the group.
        /// </summary>
        public static readonly GroupPermission Delete = nameof(Delete);

        /// <summary>
        /// Built-in permission that grants access to invite another person to the group.
        /// </summary>
        public static readonly GroupPermission Invite = nameof(Invite);

        /// <summary>
        /// Built-in permision that grants access to kick another person from the group.
        /// </summary>
        public static readonly GroupPermission Kick = nameof(Kick);

        /// <summary>
        /// The string value of this permission.
        /// </summary>
        [NotNull]
        public string Value { get; }

        /// <summary>
        /// Initialize a <see cref="GroupPermission"/> with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">the value of this permission</param>
        public GroupPermission([NotNull] string value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(string other) => string.Equals(Value, other, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public bool Equals(GroupPermission other) => Equals(other.Value);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case GroupPermission groupPermission:
                    return Equals(groupPermission);

                case string stringGroupPermission:
                    return Equals(stringGroupPermission);

                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => Value;

        public static implicit operator string([NotNull] GroupPermission groupPermission) => groupPermission.Value;

        public static implicit operator GroupPermission([NotNull] string value) => new GroupPermission(value);

        public static bool operator ==([NotNull] string left, GroupPermission right) => right.Equals(left);

        public static bool operator !=([NotNull] string left, GroupPermission right) => !right.Equals(left);

        public static bool operator ==(GroupPermission left, [NotNull] string right) => left.Equals(right);

        public static bool operator !=(GroupPermission left, [NotNull] string right) => !left.Equals(right);

        public static bool operator ==(GroupPermission left, GroupPermission right) => left.Equals(right);

        public static bool operator !=(GroupPermission left, GroupPermission right) => !left.Equals(right);
    }
}
