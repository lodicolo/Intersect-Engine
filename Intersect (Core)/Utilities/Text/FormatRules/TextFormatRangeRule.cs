using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Intersect.Utilities.Text.FormatRules
{
    public struct TextFormatRange
    {
        public bool IsValid => Start <= End;

        public ushort Start { get; }

        public ushort End { get; }

        public TextFormatRange(ushort start, ushort end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(ushort chr) => Start <= chr && chr <= End;

        public bool CanCombineWith(TextFormatRange range) => Contains(range.Start) || Contains(range.End);

        public TextFormatRange CombineWith(TextFormatRange range)
        {
            if (Contains(range.Start) && Contains(range.End))
            {
                return this;
            }

            return new TextFormatRange(Math.Min(Start, range.Start), Math.Max(End, range.End));
        }

        public static bool operator ==(TextFormatRange a, TextFormatRange b) => a.Start == b.Start && b.End == b.End;

        public static bool operator !=(TextFormatRange a, TextFormatRange b) => a.Start != b.Start || b.End != b.End;
    }

    public class TextFormatRangeRule : ITextFormatRule
    {
        private IReadOnlyList<TextFormatRange> SimplifiedRanges { get; }

        public bool IsValid => SimplifiedRanges.Count != 0;

        public IReadOnlyList<TextFormatRange> Ranges { get; }

        public TextFormatRangeRule(IEnumerable<TextFormatRange> ranges)
        {
            Ranges = ImmutableList.CreateRange(ranges);
            SimplifiedRanges = SimplifyRanges(ranges);
        }

        /// <inheritdoc />
        public bool Matches(string text) => text.All(c => SimplifiedRanges.Any(r => r.Contains(c)));

        public static IReadOnlyList<TextFormatRange> SimplifyRanges(IEnumerable<TextFormatRange> ranges)
        {
            var simplified = new List<TextFormatRange>();

            var sortedRanges = ranges.Where(r => r.IsValid).OrderBy(r => r.Start);

            using (var enumerator = sortedRanges.GetEnumerator())
            {
                TextFormatRange current = default;

                while (enumerator.MoveNext())
                {
                    var range = enumerator.Current;
                    if (current == default)
                    {
                        current = range;
                    }
                    else if (current.CanCombineWith(range))
                    {
                        current = current.CombineWith(range);
                    }
                    else
                    {
                        simplified.Add(current);
                        current = range;
                    }
                }

                simplified.Add(current);
            }

            var validSimplifiedRanges = simplified.Count(r => r.IsValid);

            if (validSimplifiedRanges < 1)
            {
                throw new ArgumentException("No valid ranges.", nameof(ranges));
            }

            return ImmutableList.CreateRange(simplified.Where(r => r.IsValid));
        }
    }
}
