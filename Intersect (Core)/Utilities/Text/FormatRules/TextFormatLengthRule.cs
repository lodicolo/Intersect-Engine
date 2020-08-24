using System;

using Intersect.Utilities;

namespace Intersect.Utilities.Text.FormatRules
{
    public class TextFormatLengthRule : ITextFormatRule
    {
        public bool IsValid => Minimum <= Maximum;

        public int Minimum { get; }

        public int Maximum { get; }

        public TextFormatLengthRule(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public TextFormatLengthRule(LengthConstraint lengthConstraint)
        {
            Minimum = lengthConstraint.Minimum;
            Maximum = lengthConstraint.Maximum;
        }

        /// <inheritdoc />
        public bool Matches(string text) => text == null
            ? Minimum <= text.Length && Maximum <= text.Length
            : throw new ArgumentNullException(nameof(text));
    }
}
