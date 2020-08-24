using System;
using System.Text.RegularExpressions;

namespace Intersect.Utilities.Text.FormatRules
{
    public class TextFormatPatternRule : ITextFormatRule
    {
        public bool IsValid => !string.IsNullOrEmpty(RawPattern);

        public string RawPattern { get; }

        public Regex Pattern { get; }

        public TextFormatPatternRule(string rawPattern, RegexOptions regexOptions = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(rawPattern))
            {
                throw new ArgumentNullException(nameof(rawPattern));
            }

            RawPattern = rawPattern;
            Pattern = new Regex(rawPattern, regexOptions, TimeSpan.FromSeconds(0.1));
        }

        /// <inheritdoc />
        public bool Matches(string text) => Pattern.IsMatch(text);
    }
}
