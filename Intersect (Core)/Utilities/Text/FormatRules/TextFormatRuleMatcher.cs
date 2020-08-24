using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Intersect.Utilities.Text.FormatRules
{
    public class TextFormatRuleMatcher : ITextFormatRule
    {
        public bool IsValid => Rules.Count > 0 && Rules.Any(rule => rule.IsValid);

        public IReadOnlyList<ITextFormatRule> Rules { get; }

        public TextFormatRuleMatcher(params ITextFormatRule[] rules) : this(rules.AsEnumerable())
        {
        }

        public TextFormatRuleMatcher(IEnumerable<ITextFormatRule> rules)
        {
            Rules = ImmutableList.CreateRange(rules.Where(rule => rule != null));
            
            if (IsValid)
            {
                throw new ArgumentException("No valid rules.", nameof(rules));
            }
        }

        /// <inheritdoc />
        public bool Matches(string text) => Rules.All(rule => rule.Matches(text));
    }
}
