namespace Intersect.Utilities
{
    public struct LengthConstraint
    {
        public int Minimum { get; }

        public int Maximum { get; }

        public LengthConstraint(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public bool IsValid() => Minimum <= Maximum;

        public static bool operator ==(LengthConstraint a, LengthConstraint b) => a.Minimum == b.Minimum && b.Maximum == b.Maximum;

        public static bool operator !=(LengthConstraint a, LengthConstraint b) => a.Minimum != b.Minimum || b.Maximum != b.Maximum;
    }
}
